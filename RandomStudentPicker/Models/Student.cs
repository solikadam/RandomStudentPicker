using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RandomStudentPicker.Models;

public class Student
{
    public string Name { get; set; }
    public bool IsSelected { get; set; } // This should exist in the Student model.
}