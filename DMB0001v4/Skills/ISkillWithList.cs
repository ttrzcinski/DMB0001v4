using System.Collections.Generic;

namespace DMB0001v4.Skills
{
    /// <summary>
    /// Keeps structure of skills containing collections.
    /// </summary>
    /// <typeparam name="T">wanted model's class</typeparam>
    public interface ISkillWithList<T> : ISkill
    {
        bool Add(T item);

        bool AddAll(List<T> items);

        T Get(int key);

        List<T> GetAll(List<int> keys);

        bool Update(int key, T item);

        bool Remove(int key);

        bool RemoveAll(List<int> keys);

        void initItems();

        void Clear();

        new int Count();

        bool IsEmpty();

        int MaxId();

        void FixMaxId();

        bool Persist();
    }
}
