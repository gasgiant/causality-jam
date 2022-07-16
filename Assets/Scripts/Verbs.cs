using System.Collections.Generic;
using UnityEngine;

public abstract class Verb
{
	public virtual string Name() => "";
	public abstract int DieCount();
	public virtual int EnergyCost() => 0;

	public abstract string Description(bool b, int startDieIndex, DiceSequence sequence);

	public abstract void Execute(DiceSequence sequence, int targetIndex, int selfIndex, Player player, List<Enemy> enemies);

	public static string DiceText(int die)
	{
		int index = Game.DieSpriteIndex(die);
		return $"<size=1><sprite={index}></size>";
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
		return null;
	}
}

public enum VerbType
{
	Wait,
	EnemyDamage,
	Sword,
	Whip
}

public class EnemyDamage : Verb
{
	public override int DieCount() => 1;

	public override void Execute(DiceSequence sequence, int targetIndex, int selfIndex, Player player, List<Enemy> enemies)
	{
		var damage = new Damage(sequence.ConsumeDie());
		Game.DealDamage(damage, ref player.health);
	}

	public override string Description(bool isPreview, int startDieIndex, DiceSequence sequence)
	{
		int die = -1;
		if (isPreview)
			die = sequence.PeekDie(startDieIndex);
		return $"Deal {DiceText(die)} damage.";
	}
}

public class Sword : Verb
{
	public override string Name() => "Sword";
	public override int EnergyCost() => 1;
	public override int DieCount() => 1;

	public override void Execute(DiceSequence sequence, int targetIndex, int selfIndex, Player player, List<Enemy> enemies)
	{
		Game.SpendEnergy(EnergyCost(), ref player.energy);
		var damage = new Damage(sequence.ConsumeDie());
		Game.DealDamage(damage, ref enemies[targetIndex].health);
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
	public override int DieCount() => 1;

	public override void Execute(DiceSequence sequence, int targetIndex, int selfIndex, Player player, List<Enemy> enemies)
	{
		Game.SpendEnergy(EnergyCost(), ref player.energy);
		sequence.ConsumeDie();
		Game.AddExtraEnergyNextTurn(1, ref player.energy);
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
	public override int DieCount() => 1;

	public override void Execute(DiceSequence sequence, int targetIndex, int selfIndex, Player player, List<Enemy> enemies)
	{
		Game.SpendEnergy(EnergyCost(), ref player.energy);
		var damage = new Damage(sequence.ConsumeDie());
		for (int i = 0; i < enemies.Count; i++)
		{
			Game.DealDamage(damage, ref enemies[i].health);
		}
	}

	public override string Description(bool isPreview, int startDieIndex, DiceSequence sequence)
	{
		int die = -1;
		if (isPreview)
			die = sequence.PeekDie(startDieIndex);
		return $"Deal {DiceText(die)} damage\nto all enemies.";
	}
}

