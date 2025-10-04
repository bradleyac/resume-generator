namespace RGS.Backend.Shared.Models;

public record Job(string Title, string Company, string Location, string Start, string End, string[] Bullets);

public record Project(string Name, string Description, string[] Technologies, string When);

public record Education(string Degree, string School, string Location, string Graduation);

public record Contact(string Email, string Phone, string Github);

public record SkillCategory(string Label, string[] Items);

public record ResumeData(string id,
                         string Name,
                         string Title,
                         string About,
                         string City,
                         string State,
                         Contact Contact,
                         Job[] Jobs,
                         Project[] Projects,
                         Education[] Education,
                         SkillCategory[] Skills);