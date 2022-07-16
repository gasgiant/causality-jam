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

	public static string DiceText(int die)
	{
		int index = Encounter.DieSpriteIndex(die);
		return $"<size=1> <sprite={index}> </size>";
	}

	public static Verb Make(VerbType type)
	{
		if (type == VerbType.Wait)
			return new Wait();
		if (type == VerbType.EnemyDamage)
			return new EnemyDamage();
		if (type == VerbType.Sword)
			return new Sword();
		if (type == VerbType.Whip)
			return new Whip();
		if (type == VerbType.EnemyDamageOnSix)
			return new EnemyDamageOnSix();
		return null;
	}
}

public enum VerbType
{
	Wait,
	EnemyDamage,
	Sword,
	Whip,
	EnemyDamageOnSix
}

public class EnemyDamage : Verb
{
	public override int DiceCount() => 1;

	public override void Execute(DiceSequence sequence, int targetIndex, int selfIndex, Player player, List<Enemy> enemies)
	{
		var damage = new Damage(sequence.ConsumeDie());
		Encounter.DealDamage(damage, ref player.health);
	}

	public override string Description(bool isPreview, int startDieIndex, DiceSequence sequence)
	{
		int die = -1;
		if (isPreview)
			die = sequence.PeekDie(startDieIndex);
		return $"Deal{DiceText(die)}damage.";
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
		enemies[targetIndex].view.Flash();
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

public class Wait : Verb
{
	public override string Name() => "Wait";
	public override int EnergyCost() => 1;
	public override int DiceCount() => 1;

	public override void Execute(DiceSequence sequence, int targetIndex, int selfIndex, Player player, List<Enemy> enemies)
	{
		Encounter.SpendEnergy(EnergyCost(), ref player.energy);
		sequence.ConsumeDie();
		Encounter.AddExtraEnergyNextTurn(1, ref player.energy);
	}

	public override string Description(bool isPreview, int startDieIndex, DiceSequence sequence)
	{
		int die = -1;
		if (isPreview)
			die = sequence.PeekDie(startDieIndex);
		return $"{DiceText(die)}\nGet 1 extra energy\nnext turn.";
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
			enemies[i].view.Flash();
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

