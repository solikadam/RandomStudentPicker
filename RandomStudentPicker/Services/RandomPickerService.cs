using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RandomStudentPicker.Models;

namespace RandomStudentPicker.Services
{
    public class RandomPickerService
    {
        private readonly Random _random = new();

        public Student PickRandomStudent(Class classModel)
        {
            if (classModel?.Students == null || classModel.Students.Count == 0)
                return null;

            var index = _random.Next(0, classModel.Students.Count);
            return classModel.Students[index];
        }
    }
}
