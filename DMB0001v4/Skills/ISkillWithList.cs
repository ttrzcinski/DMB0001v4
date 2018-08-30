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

        T Get(uint key);

        List<T> GetAll(List<uint> keys);

        bool Update(uint key, T item);

        bool Remove(uint key);

        bool RemoveAll(List<uint> keys);

        void InitItems();

        void Clear();

        uint Count { get; }

        bool IsEmpty();

        uint MaxId();

        void FixMaxId();

        bool Persist();
    }
}
