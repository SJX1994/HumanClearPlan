using CleverCrow.Fluid.StatsSystem.Editors;
using CleverCrow.Fluid.StatsSystem.StatsContainers;
using UnityEngine;
using UnityEngine.UI;
using Chronos.Example;
using System.Collections;
using DamageNumbersPro;

namespace CleverCrow.Fluid.StatsSystem {
	[RequireComponent(typeof(StatsContainerExample))]
	public class CharacterExample : ExampleBaseBehaviour {
        private StatsContainer stats;
	
        [SerializeField]
        private Slider healthBar;
        public JellyChecker checker;
	  public CharacterExample target;
	  public float hitDelay,hitRound;
	  public DamageNumber numberPrefab;
	  public Camera cam;
	  public Transform spawnPopup;
	  [HideInInspector]public bool hitOnce = false;
        void Start () {
			stats = GetComponent<StatsContainerExample>().copy;

			var health = stats.GetStat("health");
			healthBar.maxValue = health;
			healthBar.value = health;
			// 攻击逻辑
			checker = checker.GetComponent<JellyChecker>();
			target = target.GetComponent<CharacterExample>();
			hitOnce = false;
			StartCoroutine(AutoAttack());
		}
		IEnumerator AutoAttack ()
		{
			yield return time.WaitForSeconds(hitDelay);
			while(true)
			{
				RaycastHit hit = checker.HitEffect();
				if(hit.collider && hitOnce == false)
				{
					if(hit.collider.transform.parent.GetComponent<CharacterExample>() != null)
					{
						AttackTarget(target);
						hitOnce = true;
					}
					
				}
				yield return time.WaitForSeconds(hitRound);
			}
		}
		void Update () {
			healthBar.maxValue = stats.GetStat("health");
		}

		public void ReceiveDamage (int damage) {
			healthBar.value -= Mathf.Max(0, damage - stats.GetStat("armor"));
			// 飘字
			DamageNumber damageNumber = numberPrefab.Spawn(spawnPopup.position, damage + Random.Range(0,5));
			// 销毁
			if (healthBar.value < 1) {
				Destroy(gameObject);
			}
		}

		public void AttackTarget (CharacterExample target) {
			if (target == null) return;
			Debug.Log(transform.name + "Attacking " + target.name);
			target.ReceiveDamage((int)stats.GetStat("attack"));
		}

		public void AddHealth (float amount) {
			var health = stats.GetModifier(OperatorType.Add, "health", "example");
			stats.SetModifier(OperatorType.Add, "health", "example", health + amount);
		}

		public void RemoveHealth (float amount) {
			var health = stats.GetModifier(OperatorType.Subtract, "health", "example");
			stats.SetModifier(OperatorType.Subtract, "health", "example", health + amount);
		}

		public void MultiplyHealth (float amount) {
			var health = stats.GetModifier(OperatorType.Multiply, "health", "example");
			stats.SetModifier(OperatorType.Multiply, "health", "example", health + amount);
		}

		public void DivideHealth (float amount) {
			var health = stats.GetModifier(OperatorType.Divide, "health", "example");
			stats.SetModifier(OperatorType.Divide, "health", "example", health + amount);
		}

		public void RefillHealth () {
			healthBar.value = healthBar.maxValue;
		}

		public void ClearAll () {
			stats.ClearAllModifiers(stats.GetRecord("health"), "example");
		}
	}
}
