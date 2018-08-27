using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMB0001v4.Skills
{
    /// <summary>
    /// Keeps structure of skills containing collections.
    /// </summary>
    /// <typeparam name="T">wanted model's class</typeparam>
    public interface ISkillWithList<T> : ISkill
    {
        bool Add(T item);

        bool Remove(string key);

        int Count();

        bool AddAll(List<T> items);

        bool RemoveAll(List<T> items);
    }
}
