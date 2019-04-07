using System;
using System.Collections.Generic;
using GameStateComponents;

namespace Server
{
	public class AbilityEffects
	{		
		public delegate void Ability(Actor useActor, Actor hitActor);

		public List<Ability> abilities { get; }
		// here in case we need to access from different pool
		public List<Ability> basicAbilities { get; }
		public List<Ability> normalAbilities { get; }
		public List<Ability> ultimateAbilities { get; }
		
        private const int TOWER_DAMAGE = 100;

		public delegate void ApplyAbilityEffect(Actor useActor, Actor hitActor);

		public static ApplyAbilityEffect[] Apply = new ApplyAbilityEffect[] {
			// TestProjectile
			(useActor, hitActor) => {hitActor.TakeDamage(useActor, 50);},
			// TestTargeted
			(useActor, hitActor) => {hitActor.TakeDamage(useActor, 500);},
			// TestHomingTargeted
			(useActor, hitActor) => {hitActor.TakeDamage(useActor, 100);},
			// TestAreaOfEffect
			(useActor, hitActor) => {hitActor.TakeDamage(useActor, 200);},
			// AutoAttack
			(useActor, hitActor) => {hitActor.TakeDamage(useActor, 50);},
			// Wall
			(useActor, hitActor) => {},
			// Banish
			(useActor, hitActor) => {
				Random rnd = new Random();
				GameUtility.Coordinate randomPosition = new GameUtility.Coordinate(rnd.Next(100,400), rnd.Next(100,400));
				hitActor.Position = randomPosition;
				hitActor.TargetPosition = randomPosition;
			},
			// Bullet Ability
			(useActor, hitActor) => {hitActor.TakeDamage(useActor, 250);},
			// Pork Chop
			(useActor, hitActor) => {hitActor.TakeDamage(useActor, 250);},
			// Dart
			(useActor, hitActor) => {hitActor.TakeDamage(useActor, 20);},
			// Purification
			(useActor, hitActor) => {hitActor.Health+=250;},
			// UwuImScared
			(useActor, hitActor) => {hitActor.invincible=true;
									 hitActor.startInvincibilityTimer();},
			// Fireball
			(useActor, hitActor) => {hitActor.TakeDamage(useActor, 300);},
            //WeebOut
            (useActor, hitActor) => {hitActor.TakeDamage(useActor, 50);},
            //Whale
            (useActor, hitActor) => {hitActor.Health+=400;},
			//TowerAttack
			(useActor, hitActor) => {hitActor.TakeDamage(useActor, TOWER_DAMAGE); },
			//Blink
			(useActor, hitActor) => {},
			//PewPew
			(useActor, hitActor) => {hitActor.TakeDamage(useActor, 100);},
			//Sploosh
			(useActor, hitActor) => {hitActor.TakeDamage(useActor, 75);},
			// Gungnir
			(useActor, hitActor) => {hitActor.TakeDamage(useActor, 300);},
			// Slash
			(useActor, hitActor) => {hitActor.TakeDamage(useActor, 400);}
        };

		private AbilityEffects ()
		{
			abilities = new List<Ability>();
			basicAbilities = new List<Ability>();
			normalAbilities = new List<Ability>();
			ultimateAbilities = new List<Ability>();
			PopulateAbilities();
		}

		private void PopulateAbilities()// when we start using this format, make sure to add to abilityAffects[]
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
		
		public static int ReturnRandomAbilityId(Player player)
		{
			Random random = new Random();
			return 19;
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
		private static void PewPew(Actor useActor, Actor hitActor) {}
		private static void Sploosh(Actor useActor, Actor hitActor) {}
		private static void Dart(Actor useActor, Actor hitActor) {}
		private static void WeebOut(Actor useActor, Actor hitActor) {}
		private static void Slash(Actor useActor, Actor hitActor) {}
		private static void HealHarm(Actor useActor, Actor hitActor) {}
        
		//Normal Abilities
		private static void SonicBoom(Actor useActor, Actor hitActor) {}
		private static void Purification(Actor useActor, Actor hitActor) {}
		private static void Blink(Actor useActor, Actor hitActor) {}
		private static void Charrrge(Actor useActor, Actor hitActor) {}
		private static void UwuImScared(Actor useActor, Actor hitActor) {}
		private static void Wall(Actor useActor, Actor hitActor) {}
		private static void GiveUp(Actor useActor, Actor hitActor) {}
		private static void GoHome(Actor useActor, Actor hitActor) {}
		private static void Bullet(Actor useActor, Actor hitActor) {}
		private static void DarkOrb(Actor useActor, Actor hitActor) {}
		private static void Banish(Actor useActor, Actor hitActor) {}
		private static void Bless(Actor useActor, Actor hitActor) {}
		private static void VampiricLance(Actor useActor, Actor hitActor) {}
		private static void FrostBurst(Actor useActor, Actor hitActor) {}
		private static void PorkChop(Actor useActor, Actor hitActor) {}
        
		//Ultimate Abilities
		private static void Fireball(Actor useActor, Actor hitActor) {}
		private static void Gungnir(Actor useActor, Actor hitActor) {}
		private static void SelfSacrifice(Actor useActor, Actor hitActor) {}
		private static void Whale(Actor useActor, Actor hitActor) {}
		private static void Curse(Actor useActor, Actor hitActor) {}
		private static void Hack(Actor useActor, Actor hitActor) {}
		private static void Rob(Actor useActor, Actor hitActor) {}
		private static void Refresh(Actor useActor, Actor hitActor) {}
		private static void MagicalGirlTransform(Actor useActor, Actor hitActor) {}
	}
}
