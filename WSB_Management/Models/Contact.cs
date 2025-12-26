using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace WSB_Management.Models;

public class Contact
{
    public long Id { get; set; }
    public string Firstname { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Phonenumber { get; set; } = string.Empty;
}