using System;
using System.Collections.Generic;

namespace L47_war
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Battlefield battlefield = new Battlefield();

            battlefield.Fight();
        }
    }

    class Battlefield
    {
        private List<FighterCreator> _listFighters;

        private Platoon _platoon1;
        private Platoon _platoon2;

        private int _minSoldiersInPlatoon = 10;
        private int _maxSoldiersInPlatoon = 60;
        private int _countFights = 1;
        private int _maxLenghtPlatoon;
        private int _delimeterLenght = 55;

        private char _delimeterSymbol = '=';

        public Battlefield()
        {
            _listFighters = new List<FighterCreator>() { new FighterCreatorMage(),
                                                         new FighterCreatorWarior(),
                                                         new FighterCreatorBarbarian(),
                                                         new FighterCreatorPaladin(),
                                                         new FighterCreatorArcher()};

            _platoon1 = new Platoon("Австрия", RandomGenerator.GetRandomNumber(_minSoldiersInPlatoon, _maxSoldiersInPlatoon + 1), _listFighters);
            _platoon2 = new Platoon("Чехия", RandomGenerator.GetRandomNumber(_minSoldiersInPlatoon, _maxSoldiersInPlatoon + 1), _listFighters);
        }

        public void Fight()
        {
            bool isFighting = true;

            while (isFighting)
            {
                _maxLenghtPlatoon = _platoon1.Size > _platoon2.Size ? _platoon1.Size : _platoon2.Size;

                for (int round = 0; round < _maxLenghtPlatoon; round++)
                {
                    _platoon1.Attack(_platoon2, round);
                    _platoon2.Attack(_platoon1, round);
                }

                if (_platoon1.Size <= 0 || _platoon2.Size <= 0)
                {
                    isFighting = false;
                    continue;
                }

                Console.WriteLine(new string(_delimeterSymbol, _delimeterLenght) + $"\nИтоги раунда №{_countFights}:");
                Console.WriteLine($"У стороны {_platoon1.Country} осталось бойцов: {_platoon1.Size}\nУ стороны {_platoon2.Country}" +
                                  $" осталось бойцов: {_platoon2.Size}\n" + new string(_delimeterSymbol, _delimeterLenght));
                _countFights++;

                Console.WriteLine("Для продолжения нажмите любую клавишу...");
                Console.ReadKey(true);
            }

            Console.WriteLine(new string(_delimeterSymbol, _delimeterLenght) + $"\nПо итогу раунда №{_countFights}:");

            if (_platoon1.Size <= 0 && _platoon2.Size <= 0)
                Console.WriteLine("Ничья! Оба отряда разбиты.");
            else if (_platoon1.Size <= 0)
                Console.WriteLine($"{_platoon2.Country} победила. У нее бойцов осталось: {_platoon2.Size}");
            else
                Console.WriteLine($"{_platoon1.Country} победила. У нее бойцов осталось: {_platoon1.Size}");

            Console.WriteLine(new string(_delimeterSymbol, _delimeterLenght));
        }
    }

    class Platoon
    {
        private List<Fighter> _soldiers = new List<Fighter>();

        public Platoon(string country, int countSoldiers, IReadOnlyList<FighterCreator> listFighters)
        {
            Country = country;

            for (int i = 0; i < countSoldiers; i++)
            {
                int randomType = RandomGenerator.GetRandomNumber(listFighters.Count);
                _soldiers.Add(listFighters[randomType].Create());
            }
        }

        public string Country { get; private set; }

        public int Size => _soldiers.Count;

        public void Attack(Platoon enemyPlatoon, int index)
        {
            if (Size > index)
            {
                int randomIndex = RandomGenerator.GetRandomNumber(enemyPlatoon.Size);

                if (enemyPlatoon.TryGetSoldierAt(randomIndex, out Fighter underAttackSoldier))
                {
                    Fighter attackingSoldier = _soldiers[index];
                    attackingSoldier.Attack(underAttackSoldier);

                    if (underAttackSoldier.CurrentHealth <= 0)
                        enemyPlatoon.RemoveSoldier(underAttackSoldier);
                }
            }
        }

        private bool TryGetSoldierAt(int index, out Fighter fighter)
        {
            if (Size > 0)
            {
                fighter = _soldiers[index];
                return true;
            }

            fighter = null;
            return false;
        }

        private void RemoveSoldier(Fighter fighter)
        {
            _soldiers.Remove(fighter);
        }
    }

    abstract class Fighter
    {
        protected int MaxHealth;
        protected int Damage;
        protected int Armor;

        public Fighter(string type, int helthPoint, int damage, int armor)
        {
            TypeFighters = type;
            CurrentHealth = helthPoint;
            MaxHealth = helthPoint;
            Damage = damage;
            Armor = armor;
        }

        public string TypeFighters { get; protected set; }
        public int CurrentHealth { get; protected set; }

        public virtual void TakeDamage(int damage)
        {
            int calculateDamage = damage - Armor;
            calculateDamage = (calculateDamage < 0 ? 0 : calculateDamage);
            CurrentHealth -= calculateDamage;
            Console.WriteLine($"получает: {calculateDamage} ед. урона.");

            if (CurrentHealth < 0)
                CurrentHealth = 0;
        }

        public abstract void Attack(Fighter enemy);
    }

    class Mage : Fighter
    {
        private int _manaPoint;
        private int _shieldPoint;
        private Skill _skill;

        public Mage(string type, int helthPoint, int damage, int armor, int manaPoint) : base(type, helthPoint, damage, armor)
        {
            _skill = new Skill("Energy Shield", 10, 30);
            _manaPoint = manaPoint;
        }

        public override void TakeDamage(int damage)
        {
            int remainingDamage = damage - _shieldPoint;
            _shieldPoint -= damage;

            if (remainingDamage > 0)
                base.TakeDamage(remainingDamage);
            else
                Console.WriteLine($"поглощает энерго-щитом урон, у щита остается {_shieldPoint} ед. прочности.");

            if (_shieldPoint <= 0)
            {
                _skill.OnActive = false;
                _shieldPoint = 0;
            }
        }

        public override void Attack(Fighter enemy)
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
                Console.Write($"{TypeFighters} - Атакует.\t{enemy.TypeFighters} - ");
                enemy.TakeDamage(Damage);
            }
        }
    }

    class Warior : Fighter
    {
        private int _timeSkill;
        private Skill _skill;

        public Warior(string type, int helthPoint, int damage, int armor) : base(type, helthPoint, damage, armor)
        {
            _skill = new Skill("Fortify", 3, 20);
        }

        public override void TakeDamage(int damage)
        {
            base.TakeDamage(damage);
        }

        public override void Attack(Fighter enemy)
        {
            if (_timeSkill > 0)
            {
                _timeSkill--;
            }
            else
            {
                _skill.OnActive = false;
                Armor -= _skill.Power;
            }

            if (_timeSkill <= 0 && _skill.OnActive == false)
            {
                _timeSkill = _skill.Cost;
                Armor += _skill.Power;
                _skill.OnActive = true;
                Console.WriteLine($"Боец использует {_skill.Name} и увеличивает защиту на {_skill.Power} едениц.");
            }
            else
            {
                Console.Write($"{TypeFighters} - Атакует.\t{enemy.TypeFighters} - ");
                enemy.TakeDamage(Damage);
            }
        }
    }

    class Barbarian : Fighter
    {
        private int _rage;
        private int _percentageDamageAbsorbed;
        private Skill _skill;

        public Barbarian(string type, int helthPoint, int damage, int armor, int percentageDamageAbsorbed) : base(type, helthPoint, damage, armor)
        {
            _skill = new Skill("Rage", 10, 25);
            _percentageDamageAbsorbed = percentageDamageAbsorbed;
        }

        public override void TakeDamage(int damage)
        {
            int damageAbsorbed = (int)(_percentageDamageAbsorbed * ((float)damage / 100));
            _rage += damageAbsorbed;
            base.TakeDamage(damage - damageAbsorbed);
        }

        public override void Attack(Fighter enemy)
        {
            if (_rage >= _skill.Cost)
            {
                Armor -= _skill.Cost;
                Damage += _skill.Power;
                _rage -= _skill.Cost;
                _skill.OnActive = true;
                Console.WriteLine($"{TypeFighters} использует {_skill.Name} и усиливает следующий удар на {_skill.Power} едениц.");
            }

            if (_skill.OnActive)
            {
                Console.Write($"{TypeFighters} - Атакует.\t{enemy.TypeFighters} - ");
                enemy.TakeDamage(Damage);
                Armor += _skill.Cost;
                Damage -= _skill.Power;
                _skill.OnActive = false;
            }
            else
            {
                Console.Write($"{TypeFighters} - Атакует.\t{enemy.TypeFighters} - ");
                enemy.TakeDamage(Damage);
            }
        }
    }

    class Paladin : Fighter
    {
        private int _faith;
        private Skill _skill;

        public Paladin(string type, int helthPoint, int damage, int armor, int faith) : base(type, helthPoint, damage, armor)
        {
            _skill = new Skill("Heal", 10, 50);
            _faith = faith;
        }

        public override void TakeDamage(int damage)
        {
            base.TakeDamage(damage);
        }

        public override void Attack(Fighter enemy)
        {
            float half = 0.5f;
            int halfHealth = (int)(MaxHealth * half);

            if (_faith >= _skill.Cost && CurrentHealth < halfHealth)
            {
                CurrentHealth += _skill.Power;
                _faith -= _skill.Cost;
                Console.WriteLine($"Паладин использует {_skill.Name} и лечит себя на {_skill.Power} едениц.");
            }
            else
            {
                Console.Write($"{TypeFighters} - Атакует.\t{enemy.TypeFighters} - ");
                enemy.TakeDamage(Damage);
            }
        }
    }

    class Archer : Fighter
    {
        private Random _random = new Random();

        private int _maxAmmunitionCount = 100;
        private int _minAmmunitionCount = 30;
        private int _currentAmmunition;
        private int _dodgeChance;

        public Archer(string type, int helthPoint, int damage, int armor, int dodgeChance) : base(type, helthPoint, damage, armor)
        {
            _dodgeChance = dodgeChance;
            _currentAmmunition = _random.Next(_minAmmunitionCount, _maxAmmunitionCount);
        }

        public override void TakeDamage(int damage)
        {
            if (_random.Next(100) > _dodgeChance)
                base.TakeDamage(damage);
            else
                Console.WriteLine("Лучник увернулся от удара");
        }

        public override void Attack(Fighter enemy)
        {
            if (_currentAmmunition <= 0)
            {
                _dodgeChance = 0;
                float half = 0.5f;
                Damage = (int)(Damage * half);
            }

            Console.Write($"{TypeFighters} - Атакует.\t{enemy.TypeFighters} - ");
            enemy.TakeDamage(Damage);
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

    abstract class FighterCreator
    {
        public abstract Fighter Create();

        public abstract string GetTypeName();
    }

    class FighterCreatorMage : FighterCreator
    {
        public override Fighter Create() => new Mage(GetTypeName(), 200, 35, 0, 40);

        public override string GetTypeName() => "Mage";
    }

    class FighterCreatorWarior : FighterCreator
    {
        public override Fighter Create() => new Warior(GetTypeName(), 350, 15, 15);

        public override string GetTypeName() => "Warior";
    }

    class FighterCreatorBarbarian : FighterCreator
    {
        public override Fighter Create() => new Barbarian(GetTypeName(), 400, 25, 7, 30);

        public override string GetTypeName() => "Barbarian";
    }

    class FighterCreatorPaladin : FighterCreator
    {
        public override Fighter Create() => new Paladin(GetTypeName(), 300, 20, 10, 35);

        public override string GetTypeName() => "Paladin";
    }

    class FighterCreatorArcher : FighterCreator
    {
        public override Fighter Create() => new Archer(GetTypeName(), 275, 27, 5, 25);

        public override string GetTypeName() => "Archer";
    }

    static class RandomGenerator
    {
        private static Random s_random = new Random();

        public static int GetRandomNumber(int minValue, int maxValue) => s_random.Next(minValue, maxValue);

        public static int GetRandomNumber(int maxValue) => s_random.Next(maxValue);
    }
}