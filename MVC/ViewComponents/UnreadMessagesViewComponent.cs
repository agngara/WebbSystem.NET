using System.Security.Claims;
using DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;

public class UnreadMessagesViewComponent : ViewComponent
{
    private readonly UserManager<User> _userManager;
    private readonly IMessageRepository _messageRepository;

    public UnreadMessagesViewComponent(UserManager<User> userManager, IMessageRepository messageRepository)
    {
        _userManager = userManager;
        _messageRepository = messageRepository;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (!UserClaimsPrincipal.Identity!.IsAuthenticated)
            return View(0);

        var user = await _userManager.GetUserAsync(UserClaimsPrincipal);

        if (user == null)
            return View(0);

        int count = 0;
        try
        {
            count = await _messageRepository.GetUnreadCountAsync(user.Id);
        }
        catch (Exception err)
        {
            Console.WriteLine(err);
        }
        return View(count);
    }
}
