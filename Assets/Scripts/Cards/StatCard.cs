using UnityEngine;

namespace Projectiles
{
    public class StatCard
    {
        // enum for the stat of the player that is being affected
        public enum Stat
        {
            HP,
            Speed,
            JumpHeight,
            All,
            Regen
        }

        // attributes
        private int id;
        private string title;
        private string desc;
        private Stat stat_affected;
        private int value;
        private bool is_good;

        // getters
        public string Title
        {
            get { return title; }
        }

        public string Desc
        {
            get { return desc; }
        }

        public Stat StatAffected
        {
            get { return (stat_affected); }
        }

        public int Value
        {
            get { return value; }
        }

        public bool IsGood
        {
            get { return is_good; }
        }

        //constructors
        public StatCard()
        {
            stat_affected = Stat.All;
            value = 1;
        }

        public StatCard(int ID, string Title, string Desc, Stat stat, int val, bool good)
        {
            id = ID;
            title = Title;
            desc = Desc;
            stat_affected = stat;
            value = val;
            is_good = good;
        }


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
