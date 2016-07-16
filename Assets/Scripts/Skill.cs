using UnityEngine;
using System.Collections;

public class Skill {

	private float recoveryRate = 0f;
	private string name = "";
	private float cooldown = 0f;
	private float cost = 0f;


	public Skill(string n, float rec, float c) {
		name = n;
		recoveryRate = rec;
		cost = c;
	}

	public float GetCost() {
		return cost;
	}

	public string GetName() {
		return name;
	}

	public float GetRecoveryRate() {
		return recoveryRate;
	}

	public float GetCooldown() {
		return cooldown;
	}

	public void SetCooldown(float f) {
		cooldown = f;
	}

	//
	public void UpdateCooldown(float f) {
		cooldown -= f;
		if (cooldown < 0f) 
			cooldown = 0f;

	}
}
