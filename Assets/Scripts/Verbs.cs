using System.Collections.Generic;
using UnityEngine;
using CameraShake;

public abstract class Verb
{
	public virtual string Name() => "";
	public abstract int DiceCount();
	public virtual int EnergyCost() => 0;
	public virtual bool IsTargetable() => false;

	public abstract string Description(bool b, int startDieIndex, DiceSequence sequence);

	public abstract void Execute(DiceSequence sequence, int targetIndex, int selfIndex, Player player, List<Enemy> enemies);

	public static string DiceText(int die, string prefix = "")
	{
		int index = Encounter.DieSpriteIndex(die);
		return $"<size=1>{prefix}<sprite={index}></size>";
	}

	public static string EmptySmallDiceText()
	{
		int index = Encounter.DieSpriteIndex(-1);
		return $"<sprite={index}>";
	}

	public static Verb Make(EnemyAbility type)
	{
		if (type == EnemyAbility.Damage)
			return new EnemyDamage();
		if (type == EnemyAbility.DamageOnSix)
			return new EnemyDamageOnSix();
		if (type == EnemyAbility.DamageOrBlock)
			return new EnemyDamageOrBlock();
		return null;
	}
}

public enum EnemyAbility
{
	Damage,
	DamageOnSix,
	DamageOrBlock
}

public class EnemyDamage : Verb
{
	public override int DiceCount() => 1;

	public override void Execute(DiceSequence sequence, int targetIndex, int selfIndex, Player player, List<Enemy> enemies)
	{
		var damage = new Damage(sequence.ConsumeDie());
		Game.CurrentEncounter.PlayerView.TakeHit();
		enemies[selfIndex].view.Attack();
		Encounter.DealDamage(damage, ref player.health);
	}

	public override string Description(bool isPreview, int startDieIndex, DiceSequence sequence)
	{
		int die = -1;
		if (isPreview)
			die = sequence.PeekDie(startDieIndex);
		return $"Deal {DiceText(die)} damage.";
	}
}

public class EnemyDamageOnSix : Verb
{
	public override int DiceCount() => 1;

	public override void Execute(DiceSequence sequence, int targetIndex, int selfIndex, Player player, List<Enemy> enemies)
	{
		int die = sequence.ConsumeDie();
		if (die == 6)
		{
			var damage = new Damage(3);
			Game.CurrentEncounter.PlayerView.TakeHit();
			enemies[selfIndex].view.Attack();
			Encounter.DealDamage(damage, ref player.health);
		}
	}

	public override string Description(bool isPreview, int startDieIndex, DiceSequence sequence)
	{
		int die = -1;
		if (isPreview)
			die = sequence.PeekDie(startDieIndex);
		return $"{DiceText(die)} On six:\nDeal 3 damage.";
	}
}

public class EnemyDamageOrBlock : Verb
{
	public override int DiceCount() => 1;

	public override void Execute(DiceSequence sequence, int targetIndex, int selfIndex, Player player, List<Enemy> enemies)
	{
		int die = sequence.ConsumeDie();
		if (die % 2 == 0)
		{
			var damage = new Damage(die);
			Game.CurrentEncounter.PlayerView.TakeHit();
			enemies[selfIndex].view.Attack();
			Encounter.DealDamage(damage, ref player.health);
		}
		else
		{
			Encounter.AddBlock(die, ref enemies[selfIndex].health, enemies[selfIndex].view.spriteRenderer.transform.position + Vector3.up);
		}
	}

	public override string Description(bool isPreview, int startDieIndex, DiceSequence sequence)
	{
		int die = -1;
		if (isPreview)
			die = sequence.PeekDie(startDieIndex);
		return $"{DiceText(die)}\nODD: Deal {EmptySmallDiceText()} damage\nEVEN: Add {EmptySmallDiceText()} block.";
	}
}

public class Sword : Verb
{
	public override string Name() => "Sword";
	public override int EnergyCost() => 1;
	public override int DiceCount() => 1;
	public override bool IsTargetable() => true;


	public override void Execute(DiceSequence sequence, int targetIndex, int selfIndex, Player player, List<Enemy> enemies)
	{
		Encounter.SpendEnergy(EnergyCost(), ref player.energy);
		var damage = new Damage(sequence.ConsumeDie());
		Encounter.DealDamage(damage, ref enemies[targetIndex].health);
		enemies[targetIndex].view.TakeHit();
		CameraShaker.Presets.ShortShake2D();
	}

	public override string Description(bool isPreview, int startDieIndex, DiceSequence sequence)
	{
		int die = -1;
		if (isPreview)
			die = sequence.PeekDie(startDieIndex);
		return $"Deal {DiceText(die)} damage.";
	}
}

public class Shield : Verb
{
	public override string Name() => "Shield";
	public override int EnergyCost() => 1;
	public override int DiceCount() => 1;
	public override bool IsTargetable() => false;


	public override void Execute(DiceSequence sequence, int targetIndex, int selfIndex, Player player, List<Enemy> enemies)
	{
		Encounter.SpendEnergy(EnergyCost(), ref player.energy);
		int die = sequence.ConsumeDie();
		Encounter.AddBlock(die, ref player.health, Game.CurrentEncounter.PlayerView.spriteRenderer.transform.position);
	}

	public override string Description(bool isPreview, int startDieIndex, DiceSequence sequence)
	{
		int die = -1;
		if (isPreview)
			die = sequence.PeekDie(startDieIndex);
		return $"Add {DiceText(die)} block.";
	}
}

public class Wait : Verb
{
	public override string Name() => "Wait";
	public override int EnergyCost() => 1;
	public override int DiceCount() => 2;

	public override void Execute(DiceSequence sequence, int targetIndex, int selfIndex, Player player, List<Enemy> enemies)
	{
		Encounter.SpendEnergy(EnergyCost(), ref player.energy);
		sequence.ConsumeDie();
		sequence.ConsumeDie();
		Encounter.AddExtraEnergyNextTurn(1, ref player.energy);
	}

	public override string Description(bool isPreview, int startDieIndex, DiceSequence sequence)
	{
		int die0 = -1;
		int die1 = -1;
		if (isPreview)
		{
			die0 = sequence.PeekDie(startDieIndex);
			die1 = sequence.PeekDie(startDieIndex + 1);
		}
		return $"{DiceText(die0)} {DiceText(die1)}\nGet 1 extra energy\nnext turn.";
	}
}

public class Whip : Verb
{
	public override string Name() => "Whip";
	public override int EnergyCost() => 2;
	public override int DiceCount() => 1;

	public override void Execute(DiceSequence sequence, int targetIndex, int selfIndex, Player player, List<Enemy> enemies)
	{
		Encounter.SpendEnergy(EnergyCost(), ref player.energy);
		var damage = new Damage(sequence.ConsumeDie());
		for (int i = 0; i < enemies.Count; i++)
		{
			Encounter.DealDamage(damage, ref enemies[i].health);
			enemies[i].view.TakeHit();
		}
		
		CameraShaker.Presets.Explosion2D();
	}

	public override string Description(bool isPreview, int startDieIndex, DiceSequence sequence)
	{
		int die = -1;
		if (isPreview)
			die = sequence.PeekDie(startDieIndex);
		return $"Deal {DiceText(die)} damage\nto all enemies.";
	}
}

