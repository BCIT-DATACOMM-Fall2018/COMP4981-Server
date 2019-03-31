using System;
using System.Collections.Generic;
using GameStateComponents;

namespace Server
{
	public class AbilityEffects
	{
		
		public delegate void Ability();

		public List<Ability> abilities { get; }
		// here in case we need to access from different pool
		public List<Ability> basicAbilities { get; }
		public List<Ability> normalAbilities { get; }
		public List<Ability> ultimateAbilities { get; }
		
		public delegate void ApplyAbilityEffect(Actor useActor, Actor hitActor);

		public static ApplyAbilityEffect[] Apply = new ApplyAbilityEffect[] {
			// TestProjectile
			(useActor, hitActor) => {hitActor.Health-=50;},
			// TestTargeted
			(useActor, hitActor) => {hitActor.Health-=500;},
			// TestHomingTargeted
			(useActor, hitActor) => {hitActor.Health-=100;},
			// TestAreaOfEffect
			(useActor, hitActor) => {hitActor.Health-=200;},
			// UwuImScared
			(useActor, hitActor) => {
				hitActor.invincible=true;
				hitActor.Health+=200;},
			// Fireball
			(useActor, hitActor) => {hitActor.Health-=300;},
			// AutoAttack
			(useActor, hitActor) => {hitActor.Health-=50;},
			// Wall
			(useActor, hitActor) => {},
			// Banish
			(useActor, hitActor) => {
				Random rnd = new Random();
				GameUtility.Coordinate randomPosition = new GameUtility.Coordinate(rnd.Next(100,400), rnd.Next(100,400));
				hitActor.Position = randomPosition;
				hitActor.TargetPosition = randomPosition;
				}
		};
		
		private AbilityEffects ()
		{
			abilities = new List<Ability>();
			basicAbilities = new List<Ability>();
			normalAbilities = new List<Ability>();
			ultimateAbilities = new List<Ability>();
			PopulateAbilities();
		}

		private void PopulateAbilities()
		{
			abilities.Add(PewPew);
			abilities.Add(Sploosh);
			abilities.Add(Dart);
			abilities.Add(WeebOut);
			abilities.Add(Slash);
			abilities.Add(HealHarm);
            
			abilities.Add(SonicBoom);
			abilities.Add(Purification);
			abilities.Add(Blink);
			abilities.Add(Charrrge);
			abilities.Add(UwuImScared);
			abilities.Add(Wall);
			abilities.Add(GiveUp);
			abilities.Add(GoHome);
			abilities.Add(Bullet);
			abilities.Add(DarkOrb);
			abilities.Add(Banish);
			abilities.Add(Bless);
			abilities.Add(VampiricLance);
			abilities.Add(FrostBurst);
			abilities.Add(PorkChop);
        
			abilities.Add(Fireball);
			abilities.Add(Gungnir);
			abilities.Add(SelfSacrifice);
			abilities.Add(Whale);
			abilities.Add(Curse);
			abilities.Add(Hack);
			abilities.Add(Rob);
			abilities.Add(Refresh);
			abilities.Add(PewPew);
			abilities.Add(MagicalGirlTransform);

			for (var i = 0; i < abilities.Count; ++i)
			{
				if(i >= 0 && i < 6) basicAbilities.Add(abilities[i]);
				if(i >= 6 && i < 21) normalAbilities.Add(abilities[i]);
				if(i >= 21) ultimateAbilities.Add(abilities[i]);
			}
		}
		
		public int ReturnRamdomAbilityId(Player player)
		{
			Random random = new Random();
			switch (player.Level)
			{
				case 1:
					return random.Next(0, 6);
				case 2:
				case 3:
					return random.Next(6, 21);
				case 4:
					return random.Next(21, 30);
				default:
					return -1;
			}
		}

		//Basic Abilities
		private static void PewPew(){}
		private static void Sploosh(){}
		private static void Dart(){}
		private static void WeebOut(){}
		private static void Slash(){}
		private static void HealHarm(){}
        
		//Normal Abilities
		private static void SonicBoom(){}
		private static void Purification(){}
		private static void Blink(){}
		private static void Charrrge(){}
		private static void UwuImScared(){}
		private static void Wall(){}
		private static void GiveUp(){}
		private static void GoHome(){}
		private static void Bullet(){}
		private static void DarkOrb(){}
		private static void Banish(){}
		private static void Bless(){}
		private static void VampiricLance(){}
		private static void FrostBurst(){}
		private static void PorkChop(){}
        
		//Ultimate Abilities
		private static void Fireball(){}
		private static void Gungnir(){}
		private static void SelfSacrifice(){}
		private static void Whale(){}
		private static void Curse(){}
		private static void Hack(){}
		private static void Rob(){}
		private static void Refresh(){}
		private static void MagicalGirlTransform(){}
	}
}
