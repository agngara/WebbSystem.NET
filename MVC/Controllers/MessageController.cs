
using DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using MVC.Models.Message;

namespace MVC.Controllers
{
   
    public class MessageController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserRepository _userRepo;
        private readonly IConversationRepository _convRepo;
        private readonly IMessageRepository _msgRepo;
        private readonly IGuestMessageRepository _guestRepo;

        public MessageController(UserManager<User> userManager, IUserRepository userRepo, IConversationRepository convRepo, IMessageRepository msgRepo, IGuestMessageRepository guestRepo)
        {
            _userManager = userManager;
            _userRepo = userRepo;
            _convRepo = convRepo;
            _msgRepo = msgRepo;
            _guestRepo = guestRepo;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Conversation(string? searchTerm)
        {
            var isAuth = User.Identity?.IsAuthenticated == true;


            if (!isAuth)
            {
                var vm = new ConversationViewModel
                {
                    IsGuest = true,
                    SearchTerm = searchTerm,
                    UserResults = new()
                };

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var users = await _userRepo.SearchUsersAsync(searchTerm);

                    if (users.Count == 1)
                        return RedirectToAction(nameof(Chat), new { toUserId = users[0].Id });

                    vm.UserResults = users.Select(u => new UserSearchResultViewModel
                    {


                        UserId = u.Id,
                        Email = u.Email,
                        DisplayName = string.IsNullOrWhiteSpace(u.Name) ? (u.Email ?? u.UserName ?? "User") : u.Name!,
                      

                    }).ToList();

                    if (!vm.UserResults.Any())
                        ViewBag.SearchError = "No user was found";
                }

                return View(vm);
            }


            var currentUserId = _userManager.GetUserId(User)!;


            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var matches = (await _userRepo.SearchUsersAsync(searchTerm))
                    .Where(u => u.Id != currentUserId)
                    .ToList();

                if (matches.Count == 1)
                    return await OpenOrCreateAndRedirect(currentUserId, matches[0].Id);

                if (matches.Count == 0)
                    ViewBag.SearchError = "No user was found.";
            }

            var convs = await _convRepo.GetConversationsForUserAsync(currentUserId);
            var guests = await _guestRepo.GetInboxForUserAsync(currentUserId);

            var items = convs.Select((Func<Conversation, ChatViewModel>)(c =>
            {
                var other = c.Participants.FirstOrDefault(p => p.UserId != currentUserId)?.User;
                var last = c.Messages.OrderByDescending(m => m.SendDateTime).FirstOrDefault();

                return new ChatViewModel
                {
                    IsGuest = false,
                    ConversationId = c.Id,
                    OtherUserName = other?.Name,
                    OtherUserEmail = other?.Email,
                    LastMessageText = last?.Text,
                    LastMessageAt = last?.SendDateTime,
                    OtherUserProfileImageUrl = other?.Cv?.ProfileImage,


                    UnreadMessage = c.Messages.Count(m => !m.IsRead && m.SenderId != currentUserId)
                };
            }));

            var guestItems = guests.Select(g => new ChatViewModel
            {
                IsGuest = true,
                GuestId = g.Id,
                GuestName = g.GuestName,
                LastMessageText = g.Text,
                LastMessageAt = g.SentAt,
                UnreadMessage = g.IsRead ? 0 : 1
            });

            var all = items.Concat(guestItems)
                .OrderByDescending(x => x.LastMessageAt ?? DateTime.MinValue)
                .ToList();

            return View(new ConversationViewModel
            {
                IsGuest = false,
                SearchTerm = searchTerm,
                Items = all
            });
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Chat(int? id, string? toUserId)
        {
            var isAuth = User.Identity?.IsAuthenticated == true;


            if (!isAuth)
            {
                if (string.IsNullOrWhiteSpace(toUserId))
                    return RedirectToAction(nameof(Conversation));

                var toUser = await _userRepo.GetById(toUserId);
                if (toUser == null) return NotFound();

                return View(new ChatViewModel
                {
                    IsGuest = true,
                    ToUserId = toUserId,
                    OtherUserName = toUser.Name,
                    OtherUserEmail = toUser.Email
                });
            }

            if (id == null) return NotFound();
            var currentUserId = _userManager.GetUserId(User)!;

            if (!await _convRepo.UserIsParticipantAsync(id.Value, currentUserId))
                return Forbid();

            var conversation = await _convRepo.GetConversationWithMessagesAsync(id.Value);
            if (conversation == null) return NotFound();

            await _msgRepo.MarkAsReadAsync(conversation.Id, currentUserId);

            var other = conversation.Participants.FirstOrDefault(p => p.UserId != currentUserId)?.User;

            return View(new ChatViewModel
            {
                IsGuest = false,
                ConversationId = conversation.Id,
                OtherUserName = other?.Name,
                OtherUserEmail = other?.Email,
                Messages = conversation.Messages
                    .OrderBy(m => m.SendDateTime)
                    .Select(m => new MessageBubbleViewModel
                    {
                        Text = m.Text,
                        IsMine = m.SenderId == currentUserId,
                        SentAt = m.SendDateTime,
                        IsRead = m.IsRead,
                        ReadAt = m.ReadAt
                    }).ToList()
            });
        }


        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Chat(ChatViewModel model)
        {
            var isAuth = User.Identity?.IsAuthenticated == true;

            if (!isAuth)
            {
                model.IsGuest = true;

                if (string.IsNullOrWhiteSpace(model.ToUserId)) 
                {
                    return RedirectToAction(nameof(Conversation));
                }
                    

                if (string.IsNullOrWhiteSpace(model.GuestName))
                {
                    ModelState.AddModelError(nameof(model.GuestName), "Please your name");

                }

                if (string.IsNullOrWhiteSpace(model.NewMessageText))
                {
                    ModelState.AddModelError(nameof(model.NewMessageText), "U have to write a message.");

                }

             

                await _guestRepo.AddGuestMessageAsync(model.ToUserId!, model.GuestName!, model.NewMessageText!);
                return RedirectToAction(nameof(Conversation),  new { toUserId = model.ToUserId }); 
            }


            var currentUserId = _userManager.GetUserId(User)!;

            if (string.IsNullOrWhiteSpace(model.NewMessageText))
                return RedirectToAction(nameof(Chat), new { id = model.ConversationId });

            if (!await _convRepo.UserIsParticipantAsync(model.ConversationId, currentUserId))
                return Forbid();

            await _msgRepo.AddMessageAsync(model.ConversationId, currentUserId, model.NewMessageText);
            return RedirectToAction(nameof(Chat), new { id = model.ConversationId });
        }


        [HttpGet]
        public async Task<IActionResult> CreateConversation(string otherUserId)
        {
            var currentUserId = _userManager.GetUserId(User)!;
            return await OpenOrCreateAndRedirect(currentUserId, otherUserId);
        }

        private async Task<IActionResult> OpenOrCreateAndRedirect(string currentUserId, string otherUserId)
        {
            var existingId = await _convRepo.GetExistingConversationIdAsync(currentUserId, otherUserId);
            if (existingId != null)
                return RedirectToAction(nameof(Chat), new { id = existingId.Value });

            var newId = await _convRepo.CreateConversationAsync(currentUserId, otherUserId);
            return RedirectToAction(nameof(Chat), new { id = newId });
        }


        [HttpGet]
        public async Task<IActionResult> Guest(int id)
        {
            var currentUserId = _userManager.GetUserId(User)!;

            var guest = await _guestRepo.GetByIdForUserAsync(id, currentUserId);
            if (guest == null) return NotFound();

            await _guestRepo.MarkAsReadAsync(id, currentUserId);
            return View(guest);
        }
    }
}

