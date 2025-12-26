using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WSB_Management.Models;

public class Cup
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public List<Team> CupTeams { get; set; } = new List<Team>();
}
