﻿using AnimationEnums;
using System.Collections;
using System.Globalization;
using UnityEngine;

public class Kirby : StateMachineBase {
	public float speed = 6f;
	public float jumpSpeed = 12.5f;
	public float flySpeed = 7f;

	public float knockbackSpeed = 8f;
	public float knockbackTime = 0.2f;

	// TODO: This is a bad way of doing this. See KnockbackEnterState
	private Collision2D enemyOther;

	private AnimationManager am;

	// For debugging purposes
	public State curState;

	public enum State {
		IDLE_OR_WALKING, JUMPING, FLYING, KNOCKBACK, SLIDING, INHALING, INHALED
	}

	void setState(State state) {
		curState = state;
		CurrentState = state;
		am.State = (int) state;
	}

	void Start() {
		am = new AnimationManager(this.GetComponent<Animator>());
		setState(State.JUMPING);
	}

	void CommonOnCollisionEnter2D(Collision2D other) {
		if (other.gameObject.tag == "ground") {
			setState(State.IDLE_OR_WALKING);
		} else if (other.gameObject.tag == "enemy") {
			enemyOther = other;
			Destroy(other.gameObject);
			setState(State.KNOCKBACK);
		}
	}

	void HandleHorizontalMovement(ref Vector2 vel) {
		float h = Input.GetAxis("Horizontal");
		if (h > 0) {
			am.Dir = Direction.RIGHT;
		} else if (h < 0) {
			am.Dir = Direction.LEFT;
		}
		vel.x = h * speed;
	}

	#region IDLE_OR_WALKING

	void IdleOrWalkingUpdate() {
		Vector2 vel = rigidbody2D.velocity;
		HandleHorizontalMovement(ref vel);
		if (Input.GetKey(KeyCode.X)) {
			vel.y = jumpSpeed;
			setState(State.JUMPING);
		} else if (Input.GetKey(KeyCode.UpArrow)) {
			vel.y = flySpeed;
			setState(State.FLYING);
		} else {
			if (vel.x == 0) {
				am.animate((int) IdleOrWalking.IDLE);
			} else {
				am.animate((int) IdleOrWalking.WALKING);
			}
		}
		rigidbody2D.velocity = vel;
	}

	void IdleOrWalkingOnCollisionEnter2D(Collision2D other) {
		CommonOnCollisionEnter2D(other);
	}

	#endregion

	#region JUMPING

	void JumpingUpdate() {
		Vector2 vel = rigidbody2D.velocity;
		HandleHorizontalMovement(ref vel);
		if (Input.GetKeyUp(KeyCode.X)) {
			vel.y = Mathf.Min(vel.y, 0);
		}
		rigidbody2D.velocity = vel;
	}

	void JumpingOnCollisionEnter2D(Collision2D other) {
		CommonOnCollisionEnter2D(other);
	}

	#endregion

	#region FLYING

	void FlyingUpdate() {
		Vector2 vel = rigidbody2D.velocity;
		HandleHorizontalMovement(ref vel);
		if (Input.GetKey(KeyCode.X) || Input.GetKey(KeyCode.UpArrow)) {
			vel.y = flySpeed;
		} else {
			vel.y = Mathf.Max(vel.y, -1 * flySpeed);
		}
		rigidbody2D.velocity = vel;
	}

	void FlyingOnCollisionEnter2D(Collision2D other) {
		CommonOnCollisionEnter2D(other);
	}

	#endregion

	#region KNOCKBACK

	IEnumerator KnockbackEnterState() {
		float xVel = knockbackSpeed;
		if (enemyOther.transform.position.x > transform.position.x) {
			xVel *= -1;
		}
		rigidbody2D.velocity = new Vector2(xVel, 0);
		yield return new WaitForSeconds(knockbackTime);
		setState(State.IDLE_OR_WALKING);
		rigidbody2D.velocity = Vector2.zero;
	}

	#endregion

	#region SLIDING
	#endregion
}
