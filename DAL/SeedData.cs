using System;
using System.Linq;
using System.Collections.Generic;
using Models;

namespace DAL
{
    public static class SeedData
    {
        public static void Initialize(AppDbContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            // Don't seed if data already exists
            if (context.CVs.Any())
            {
                return;
            }

            var rnd = new Random(12345);

            // Institutions (used by Educations)
            var institutions = new List<Institution>
            {
                new Institution { Name = "KTH Royal Institute of Technology" },
                new Institution { Name = "Lund University" },
                new Institution { Name = "Chalmers University of Technology" },
                new Institution { Name = "Uppsala University" },
                new Institution { Name = "Stockholm University" }
            };
            context.Institutions.AddRange(institutions);
            context.SaveChanges();

            // WorkPlaces (used by WorkExperiences)
            var workplaces = new List<WorkPlace>
            {
                new WorkPlace { Name = "Acme Software AB" },
                new WorkPlace { Name = "Nordic Solutions" },
                new WorkPlace { Name = "BetaTech" },
                new WorkPlace { Name = "GreenField Consulting" },
                new WorkPlace { Name = "BlueWave Industries" }
            };
            context.WorkPlaces.AddRange(workplaces);
            context.SaveChanges();

            // Competence pool
            var competencePool = new[]
            {
                "C#", ".NET", "Entity Framework", "SQL", "JavaScript",
                "TypeScript", "React", "Angular", "Docker", "Kubernetes",
                "Azure", "CI/CD", "Unit Testing", "TDD", "REST APIs"
            };

            var users = new List<User>();
            var cvs = new List<CV>();
            var competences = new List<Competence>();
            var educations = new List<Education>();
            var workExperiences = new List<WorkExperience>();

            for (var i = 1; i <= 25; i++)
            {
                var userId = Guid.NewGuid().ToString();
                var userName = $"user{i}@example.com";
                var displayName = i switch
                {
                    <= 5 => $"Anna Svensson {i}",
                    <= 10 => $"Erik Andersson {i}",
                    <= 15 => $"Maria Larsson {i}",
                    <= 20 => $"Johan Nilsson {i}",
                    _ => $"Sofia Karlsson {i}"
                };

                var user = new User
                {
                    Id = userId,
                    UserName = userName,
                    NormalizedUserName = userName.ToUpperInvariant(),
                    Email = userName,
                    NormalizedEmail = userName.ToUpperInvariant(),
                    EmailConfirmed = true,
                    Name = displayName,
                    Adress = $"Storgatan {i}, 111 0{i} Stockholm",
                    IsPrivate = (i % 7 == 0) ? true : false,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    AccessFailedCount = 0
                };
                users.Add(user);

                var cv = new CV
                {
                    UserId = userId,
                    User = user,
                    ProfileImage = null
                };
                cvs.Add(cv);

                // Competences: 2-4 per CV
                var competenceCount = rnd.Next(2, 5);
                var compIndices = new HashSet<int>();
                while (compIndices.Count < competenceCount)
                {
                    compIndices.Add(rnd.Next(competencePool.Length));
                }

                foreach (var idx in compIndices)
                {
                    competences.Add(new Competence
                    {
                        Name = competencePool[idx],
                        Cv = cv
                    });
                }

                // Education: 1 per CV
                var institution = institutions[rnd.Next(institutions.Count)];
                var eduYears = rnd.Next(1, 6); // length of study
                var endOffsetYears = rnd.Next(0, 10);
                var endDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-endOffsetYears));
                var startDate = endDate.AddYears(-eduYears);

                educations.Add(new Education
                {
                    ProgramOrCourseName = (eduYears >= 3) ? "Bachelor of Science in Computer Science" : "Short Course in Software Development",
                    Institution = institution,
                    InstitutionId = institution.Id,
                    StartDate = startDate,
                    EndDate = endDate,
                    Description = $"Studied {eduYears} year(s) in programming and system design.",
                    Cv = cv
                });

                // WorkExperience: 1 per CV
                var workplace = workplaces[rnd.Next(workplaces.Count)];
                var workDurationYears = rnd.Next(1, 6);
                var workEndOffset = rnd.Next(0, 3);
                var workEnd = DateOnly.FromDateTime(DateTime.Today.AddYears(-workEndOffset));
                var workStart = workEnd.AddYears(-workDurationYears);

                workExperiences.Add(new WorkExperience
                {
                    Title = (workDurationYears >= 3) ? "Senior Developer" : "Junior Developer",
                    Description = $"Worked as developer for {workDurationYears} year(s) focusing on backend and APIs.",
                    StartDate = workStart,
                    EndDate = workEnd,
                    WorkPlace = workplace,
                    WorkPlaceId = workplace.Id,
                    Cv = cv
                });
            }

            // Add in correct order and save
            context.Users.AddRange(users);
            context.CVs.AddRange(cvs);
            context.Competences.AddRange(competences);
            context.Educations.AddRange(educations);
            context.WorkExperiences.AddRange(workExperiences);

            context.SaveChanges();
        }
    }
}

