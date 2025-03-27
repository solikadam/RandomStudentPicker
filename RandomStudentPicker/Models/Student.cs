using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RandomStudentPicker.Models;

public class Student
{
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public override string ToString() => $"{FirstName} {LastName}";
}