namespace RGS.Backend.Shared.Models;

public record Ranking(int id, int jobid, int wt);
public record Rankings(Ranking[] wts);
public record Bullet(int id, int jobid, string bulletText);