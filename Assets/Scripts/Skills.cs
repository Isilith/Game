using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Skills {

	List<Skill> skills = new List<Skill>();

	public void Add(string n, float rec, float cost) {
		Skill newSkill = new Skill(n, rec, cost);

		foreach(Skill s in skills) {
			if (s.GetName() == n) {
				Debug.Log("Skill with name '" + n + "' is already added.");
				return;
			}
		}
		skills.Add(newSkill);
	}

	public int GetSkillCount() {
		return skills.Count;
	}

	public void Remove(string name)  {
		foreach(Skill s in skills) {
			if (s.GetName() == name) {
				Debug.Log("Skill with name '" + name + "' was removed.");
				skills.Remove(s);
				break;
			}
		}
	}


	public Skill GetSkill(int ind) {
		if (ind >= 0 && ind < skills.Count)
			return skills[ind];
		return null;
	}

	public Skill GetSkillWithName(string n) {
		foreach(Skill s in skills) {
			if (s.GetName() == n) 
				return s;
		}
		return null;
	}


	public void Use(NetworkCharacter player, int i) {
		if (i >= 0 && i < skills.Count) {
			Skill s = skills[i];
			if (s.GetName() == "Blink") {
				if (s.GetCooldown() <= 0 && player.GetComponent<Health>().GetEnergy() > s.GetCost()) {
					Blink(player, i);
				}
			}
			else if (s.GetName() == "Heal") {
				if (s.GetCooldown() <= 0 && player.GetComponent<Health>().GetEnergy() > s.GetCost()) {
					Heal(player, i);
				}
			}
		}
	}

/*	public void Use(NetworkCharacter player, string n) {
		
	}*/



	public void UpdateSkillCooldowns(float f) {
		foreach(Skill s in skills) {
			s.UpdateCooldown(f);
		}
	}

	public void Heal(NetworkCharacter player, int i) {
		if(skills[i].GetCooldown() > 0) {
			return;
		}

		player.GetComponent<Health>().ChangeHealth(50f);
		player.GetComponent<Health>().UseEnergy(skills[i].GetCost());
		skills[i].SetCooldown(skills[i].GetRecoveryRate());
	}

	public void Blink(NetworkCharacter player, int i) {
		Vector3 orig = player.GetComponentInChildren<Camera>().transform.position;
		Vector3 dir = player.GetComponentInChildren<Camera>().transform.forward;

		if(skills[i].GetCooldown() > 0) {
			return;
		}

		Ray ray = new Ray(orig, dir);
		Transform hitTransform;
		Vector3   hitPoint;

		hitTransform = player.FindClosestHitObject(ray, out hitPoint);

		if(hitTransform != null && Vector3.Distance(hitPoint, orig) < 100f) {
			player.transform.position = hitPoint - .99f * dir;
		} else {
			player.transform.position += dir*100f;
		}

		player.GetComponent<Health>().UseEnergy(skills[i].GetCost());
		skills[i].SetCooldown(skills[i].GetRecoveryRate());
	}

}
