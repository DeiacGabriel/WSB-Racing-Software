using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WSB_Management.Models;

public class Transponder
{
    public long Id { get; set; }
    public string Bezeichung { get; set; }
    public string Number { get; set; }
}
