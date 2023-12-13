using System;
using System.Collections.Generic;

namespace L47_war
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Random random = new Random();

            Platoon platoon1;
            Platoon platoon2;

            int minSoldiersInPlatoon = 10;
            int maxSoldiersInPlatoon = 60;
            int countFights = 1;
            int maxLenghtPlatoon;
            int delimeterLenght = 55;

            char delimeter = '=';

            bool isOpen = true;

            platoon1 = new Platoon("Австрия", random.Next(minSoldiersInPlatoon, maxSoldiersInPlatoon + 1), random);
            platoon2 = new Platoon("Чехия", random.Next(minSoldiersInPlatoon, maxSoldiersInPlatoon + 1), random);

            while (isOpen)
            {
                maxLenghtPlatoon = platoon1.Size > platoon2.Size ? platoon1.Size : platoon2.Size;

                for (int round = 0; round < maxLenghtPlatoon; round++)
                {
                    platoon1.Attack(platoon2, round);
                    platoon2.Attack(platoon1, round);
                }

                if (platoon1.Size <= 0 || platoon2.Size <= 0)
                {
                    Console.WriteLine(new string(delimeter, delimeterLenght) + $"\nПо итогу раунда №{countFights}:");

                    if (platoon1.Size <= 0 && platoon2.Size <= 0)
                        Console.WriteLine("Ничья! Оба отряда разбиты.");
                    else if (platoon1.Size <= 0)
                        Console.WriteLine($"{platoon2.Country} победила. У нее бойцов осталось: {platoon2.Size}");
                    else
                        Console.WriteLine($"{platoon1.Country} победила. У нее бойцов осталось: {platoon1.Size}");

                    Console.WriteLine(new string(delimeter, delimeterLenght));

                    isOpen = false;
                    continue;
                }

                Console.WriteLine(new string(delimeter, delimeterLenght) + $"\nИтоги раунда №{countFights}:");
                Console.WriteLine($"У стороны {platoon1.Country} осталось бойцов: {platoon1.Size}\nУ стороны {platoon2.Country}" +
                                  $" осталось бойцов: {platoon2.Size}\n" + new string(delimeter, delimeterLenght));
                countFights++;

                Console.WriteLine("Для продолжения нажмите любую клавишу...");
                Console.ReadKey(true);
            }
        }
    }

    class Platoon
    {
        Random _random = new Random();
        private List<Fighter> _soldiers = new List<Fighter>();

        public Platoon(string country, int countSoldiers, Random random)
        {
            Country = country;
            _random = random;

            for (int i = 0; i < countSoldiers; i++)
            {
                int randomType = _random.Next(Enum.GetValues(typeof(TypeFighters)).Length) + 1;
                _soldiers.Add(Fighter.CreateFighter((TypeFighters)randomType));
            }
        }

        public string Country { get; private set; }

        public int Size
        {
            get
            {
                return _soldiers.Count;
            }
        }

        public void Attack(Platoon enemyPlatoon, int index)
        {
            if (Size > index)
            {
                int randomIndex = _random.Next(enemyPlatoon.Size);

                if (enemyPlatoon.TryGetSoldierAt(randomIndex, out Fighter underAttackSoldier))
                {
                    Fighter attackingSoldier = _soldiers[index];
                    attackingSoldier.Attack(underAttackSoldier);

                    if (underAttackSoldier.CurrentHelth <= 0)
                        enemyPlatoon.RemoveSoldier(underAttackSoldier);
                }
            }
        }

        public bool TryGetSoldierAt(int index, out Fighter fighter)
        {
            if (Size > 0)
            {
                fighter = _soldiers[index];
                return true;
            }

            fighter = null;
            return false;
        }

        public void RemoveSoldier(Fighter fighter)
        {
            _soldiers.Remove(fighter);
        }
    }

    class Fighter
    {
        protected TypeFighters _type;
        protected string _name;
        protected int _currentHelth;
        protected int _maxHealth;
        protected int _damage;
        protected int _armor;

        public Fighter(TypeFighters type, int helthPoint, int damage, int armor)
        {
            _type = type;
            _currentHelth = helthPoint;
            _maxHealth = helthPoint;
            _damage = damage;
            _armor = armor;
        }

        public TypeFighters TypeFighters
        {
            get
            {
                return _type;
            }
        }

        public int CurrentHelth
        {
            get
            {
                return _currentHelth;
            }
        }

        public static Fighter CreateFighter(TypeFighters type)
        {
            switch (type)
            {
                case TypeFighters.Mage:
                    return new Mage(type, 200, 35, 0, 40);

                case TypeFighters.Warior:
                    return new Warior(type, 350, 15, 15);

                case TypeFighters.Barbarian:
                    return new Barbarian(type, 400, 25, 7, 30);

                case TypeFighters.Paladin:
                    return new Paladin(type, 300, 20, 10, 35);

                case TypeFighters.Archer:
                    return new Archer(type, 275, 27, 5, 25);
                default:
                    return null;
            }
        }

        virtual public void SetDamage(int damage)
        {
            int calculateDamage = damage - _armor;
            calculateDamage = (calculateDamage < 0 ? 0 : calculateDamage);
            _currentHelth -= calculateDamage;
            Console.WriteLine($"получает: {calculateDamage} ед. урона.");

            if (_currentHelth < 0)
                _currentHelth = 0;
        }

        virtual public void Attack(Fighter enemy)
        {
            Console.Write($"{_type} - Атакует.\t{enemy.TypeFighters} - ");
        }
    }

    class Mage : Fighter
    {
        private int _manaPoint;
        private int _shieldPoint;
        private Skill _skill = new Skill("Energy Shield", 10, 30);

        public Mage(TypeFighters type, int helthPoint, int damage, int armor, int manaPoint) : base(type, helthPoint, damage, armor)
        {
            _manaPoint = manaPoint;
        }

        public override void SetDamage(int damage)
        {
            int remainingDamage = damage - _shieldPoint;
            _shieldPoint -= damage;

            if (remainingDamage > 0)
            {
                base.SetDamage(remainingDamage);
            }
            else
            {
                Console.WriteLine($"поглощает энерго-щитом урон, у щита остается {_shieldPoint} ед. прочности.");
            }

            if (_shieldPoint <= 0)
            {
                _skill.OnActive = false;
                _shieldPoint = 0;
            }
        }

        override public void Attack(Fighter enemy)
        {
            if (_manaPoint >= _skill.Cost && _shieldPoint <= 0 && _skill.OnActive == false)
            {
                _shieldPoint = _skill.Power;
                _manaPoint -= _skill.Cost;
                _skill.OnActive = true;
                Console.WriteLine($"Маг кастует на себя {_skill.Name} на {_skill.Power} едениц.");
            }
            else
            {
                base.Attack(enemy);
                enemy.SetDamage(_damage);
            }
        }
    }

    class Warior : Fighter
    {
        private int _timeSkill;
        private Skill _skill = new Skill("Fortyfy", 3, 20);

        public Warior(TypeFighters type, int helthPoint, int damage, int armor) : base(type, helthPoint, damage, armor) { }

        public override void SetDamage(int damage)
        {
            base.SetDamage(damage);
        }

        override public void Attack(Fighter enemy)
        {
            if (_timeSkill > 0)
            {
                _timeSkill--;
            }
            else
            {
                _skill.OnActive = false;
                _armor -= _skill.Power;
            }

            if (_timeSkill <= 0 && _skill.OnActive == false)
            {
                _timeSkill = _skill.Cost;
                _armor += _skill.Power;
                _skill.OnActive = true;
                Console.WriteLine($"Боец использует {_skill.Name} и увеличивает защиту на {_skill.Power} едениц.");
            }
            else
            {
                base.Attack(enemy);
                enemy.SetDamage(_damage);
            }
        }
    }

    class Barbarian : Fighter
    {
        private int _rage;
        private int _percentageDamageAbsorbed;
        private Skill _skill = new Skill("Rage", 10, 25);

        public Barbarian(TypeFighters type, int helthPoint, int damage, int armor, int percentageDamageAbsorbed) : base(type, helthPoint, damage, armor)
        {
            _percentageDamageAbsorbed = percentageDamageAbsorbed;
        }

        public override void SetDamage(int damage)
        {
            int damageAbsorbed = (int)(_percentageDamageAbsorbed * ((float)damage / 100));
            _rage += damageAbsorbed;
            base.SetDamage(damage - damageAbsorbed);
        }

        override public void Attack(Fighter enemy)
        {
            if (_rage >= _skill.Cost)
            {
                _armor -= _skill.Cost;
                _damage += _skill.Power;
                _rage -= _skill.Cost;
                _skill.OnActive = true;
                Console.WriteLine($"{_type} использует {_skill.Name} и усиливает следующий удар на {_skill.Power} едениц.");
            }

            if (_skill.OnActive)
            {
                base.Attack(enemy);
                enemy.SetDamage(_damage);
                _armor += _skill.Cost;
                _damage -= _skill.Power;
                _skill.OnActive = false;
            }
            else
            {
                base.Attack(enemy);
                enemy.SetDamage(_damage);
            }
        }
    }

    class Paladin : Fighter
    {
        private int _faith;
        private Skill _skill = new Skill("Heal", 10, 50);

        public Paladin(TypeFighters type, int helthPoint, int damage, int armor, int faith) : base(type, helthPoint, damage, armor)
        {
            _faith = faith;
        }

        public override void SetDamage(int damage)
        {
            base.SetDamage(damage);
        }

        override public void Attack(Fighter enemy)
        {
            if (_faith >= _skill.Cost && _currentHelth < (_maxHealth / 2))
            {
                _currentHelth += _skill.Power;
                _faith -= _skill.Cost;
                Console.WriteLine($"Паладин использует {_skill.Name} и лечит себя на {_skill.Power} едениц.");
            }
            else
            {
                base.Attack(enemy);
                enemy.SetDamage(_damage);
            }
        }
    }

    class Archer : Fighter
    {
        private Random _random = new Random();
        private int _dodgeChance;
        private int _ammunition;

        public Archer(TypeFighters type, int helthPoint, int damage, int armor, int dodgeChance) : base(type, helthPoint, damage, armor)
        {
            _dodgeChance = dodgeChance;
            _ammunition = _random.Next(30, 100);
        }

        public override void SetDamage(int damage)
        {
            if (_random.Next(100) > _dodgeChance)
                base.SetDamage(damage);
            else
                Console.WriteLine("Лучник увернулся от удара");
        }

        override public void Attack(Fighter enemy)
        {

            if (_ammunition <= 0)
            {
                _dodgeChance = 0;
                _damage /= 2;
            }

            base.Attack(enemy);
            enemy.SetDamage(_damage);
        }
    }

    class Skill
    {
        public Skill(string nameSkill, int costSkill, int powerSkill)
        {
            Name = nameSkill;
            Cost = costSkill;
            Power = powerSkill;
            OnActive = false;
        }

        public string Name { get; private set; }

        public int Cost { get; private set; }

        public int Power { get; private set; }

        public bool OnActive { get; set; }
    }

    enum TypeFighters
    {
        Mage = 1,
        Warior = 2,
        Barbarian = 3,
        Paladin = 4,
        Archer = 5,
    }
}
