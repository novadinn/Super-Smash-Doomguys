using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour {

	const float kMoveSpeed = 8f;

	const float kAccelerationTimeGrounded = .1f;
	const float kAccelerationTimeAirborne = .2f;

	const float kGravity = 50f;
	const float kJumpGravity = 30f;

	const float kJumpVelocity = 15f;
	const float kDoubleJumpVelocity = 10f;

	const float kWeaponHeight = 0.35f;

	[SerializeField] LayerMask collision_mask_;
	[SerializeField] GameObject spawn_effect_;
	[SerializeField] GameObject death_effect_;
	bool was_init_ = false;

	[SerializeField] Config config_;
	[SerializeField] Team team_;

	BoxCollider2D collider_;
	Animator animator_;
	SpriteRenderer sprite_renderer_;
	Camera camera_;

	Vector2 velocity_;
	Vector2 move_direction_;
	float velocity_x_smoothing_;

	bool jump_active_;
	bool double_jump_active_;

	[SerializeField] Weapon start_weapon_;
	Weapon current_weapon_;
	Weapon other_weapon_;

	class CollisionSystem {
		const float kSkinWidth = .015f;
		const int kHorizontalRayCount = 30;
		const int kVerticalRayCount = 20;

		public bool below { get; private set; }
		public bool above { get; private set; }

		BoxCollider2D collider_;
		LayerMask collision_mask_;
		float horizontal_ray_spacing_;
		float vertical_ray_spacing_;

		public CollisionSystem(BoxCollider2D collider, LayerMask collision_mask) {
			collider_ = collider;
			collision_mask_ = collision_mask;

			Bounds bounds = collider_.bounds;
			bounds.Expand(kSkinWidth * -2);
			horizontal_ray_spacing_ = bounds.size.y / (kHorizontalRayCount - 1);
			vertical_ray_spacing_ = bounds.size.x / (kVerticalRayCount - 1);
		}

		public Vector2 move(Vector2 delta_velocity) {
			Bounds bounds = collider_.bounds;
			bounds.Expand (kSkinWidth * -2);
			Vector2 bottom_left = new Vector2 (bounds.min.x, bounds.min.y);
			Vector2 bottom_right = new Vector2 (bounds.max.x, bounds.min.y);
			Vector2 top_left = new Vector2 (bounds.min.x, bounds.max.y);
			above = below = false;
			//info.left = info.right = false;

			if (delta_velocity.x != 0) {
				float direction_x = Mathf.Sign (delta_velocity.x);
				float ray_length = Mathf.Abs (delta_velocity.x) + kSkinWidth;
				
				for (int i = 0; i < kHorizontalRayCount; i ++) {
					Vector2 ray_origin = (direction_x == -1)?bottom_left:bottom_right;
					ray_origin += Vector2.up * (horizontal_ray_spacing_ * i);
					RaycastHit2D hit = Physics2D.Raycast(ray_origin, Vector2.right * direction_x, ray_length, collision_mask_);

					if (hit) {
						delta_velocity.x = (hit.distance - kSkinWidth) * direction_x;
						ray_length = hit.distance;

						//info.left = direction_x == -1;
						//info.right = direction_x == 1;
					}
				}
			}
			if (delta_velocity.y != 0) {
				float direction_y = Mathf.Sign (delta_velocity.y);
				float ray_length = Mathf.Abs (delta_velocity.y) + kSkinWidth;

				for (int i = 0; i < kVerticalRayCount; i ++) {
					Vector2 ray_origin = (direction_y == -1)?bottom_left:top_left;
					ray_origin += Vector2.right * (vertical_ray_spacing_ * i);
					RaycastHit2D hit = Physics2D.Raycast(ray_origin, Vector2.up * direction_y, ray_length, collision_mask_);

					if (hit) {
						delta_velocity.y = (hit.distance - kSkinWidth) * direction_y;
						ray_length = hit.distance;

						below = direction_y == -1;
						above = direction_y == 1;
					}
				}
			}

			return delta_velocity;
		}
	}
	class PlayerStats { // TODO: create 1 generics stat class
		const float kMaxHealth = 200;
		const float kMaxArmor = 200;
		const float kArmorDecrementor = 4;
		const float kHealthDecrementor = 3;

		float current_health_;
		float current_armor_;

		public PlayerStats() {
			current_health_ = kMaxHealth/2;
			current_armor_ = kMaxArmor/2;
		}

		public void takeDamage(float damage) {
			float previous_armor = current_armor_; // TODO: процент считается не правильно
			current_armor_ = Mathf.Max(0, current_armor_-damage/kArmorDecrementor*kHealthDecrementor);
			float difference = (previous_armor - current_armor_)/kHealthDecrementor;

			current_health_ -= difference == 0 ? damage : difference;
		}

		public bool died() {
			return current_health_ < 0.0f;
		}

		public bool takeHealth(float health) {
			if(current_health_ == kMaxHealth) {
				return false;
			}

			current_health_ = Mathf.Min(current_health_ + health, kMaxHealth);
			return true;
		}

		public bool takeArmor(float armor) {
			if(current_armor_ == kMaxArmor) {
				return false;
			}
			
			current_armor_ = Mathf.Min(current_armor_ + armor, kMaxArmor);
			return true;
		}
	}
	
	CollisionSystem collision_system_;
	PlayerStats player_stats_;
	
	void Awake() {
		animator_ = GetComponent<Animator>();
		sprite_renderer_ = GetComponent<SpriteRenderer>();
		collider_ = GetComponent<BoxCollider2D>();
		camera_ = Camera.main;
	}

	void Start() {
		collision_system_ = new CollisionSystem(collider_, collision_mask_);
		player_stats_ = new PlayerStats();

		StartCoroutine("spawn");
	}

	void Update() {
		if(!was_init_)
			return;

		// handleInput();
		if (Input.GetKeyDown (config_.jumpKey())) {
			jump_active_ = true;
			if(collision_system_.below) {
				velocity_.y = kJumpVelocity;
			} else if(double_jump_active_) {
				velocity_.y = kDoubleJumpVelocity;
				double_jump_active_ = false;
			}
		} else if(Input.GetKeyUp(config_.jumpKey())) {
			jump_active_ = false;
		}

		if(Input.GetKey(config_.attackKey())) {
			if(current_weapon_) {
				current_weapon_.attack(team_);
			}
		} else if(Input.GetKeyDown(config_.swapKey())) {
			Weapon temp = current_weapon_;
			current_weapon_ = other_weapon_;
			other_weapon_ = temp;
			if(other_weapon_)
				other_weapon_.toggleActive(false);
			if(current_weapon_)
				current_weapon_.toggleActive(true);
		}

		if(current_weapon_) {
			if(Input.GetKey(config_.destroyKey())) {
				current_weapon_.startDestroy();
			} else {
				current_weapon_.stopDestroy();
			}
		}
		
		// updateMovement();
		Vector2 position = (Vector2)transform.position - new Vector2(0, kWeaponHeight);
		Vector2 direction = config_.getDirection(camera_, transform.position);
		if(current_weapon_) {
			current_weapon_.setTransform(position, direction);
		}
		if(other_weapon_) {
			other_weapon_.setTransform(position, direction);
		}
		
		move_direction_ = new Vector2(Input.GetAxisRaw(config_.getHorizontalAxis()), Input.GetAxisRaw(config_.getVerticalAxis()));

		float acceleration_time = collision_system_.below ? kAccelerationTimeGrounded : kAccelerationTimeAirborne;
		velocity_.x = Mathf.SmoothDamp(velocity_.x, move_direction_.x * kMoveSpeed, ref velocity_x_smoothing_, acceleration_time);

		float gravity = jump_active_ ? -kJumpGravity : -kGravity;
		velocity_.y += gravity * Time.deltaTime;

		transform.Translate(collision_system_.move(velocity_ * Time.deltaTime));

		if(collision_system_.below) {
			double_jump_active_ = true;
		}
		if (collision_system_.above || collision_system_.below) {
			velocity_.y = 0;
		}

		// updateAnimations();
		if(move_direction_.x != 0) {
			sprite_renderer_.flipX = move_direction_.x < 0;
		}

		animator_.SetFloat("velocityY",velocity_.y);
		animator_.SetBool("isGrounded", collision_system_.below);
		animator_.SetBool("isRunning", move_direction_.x != 0);
	}

	public int pickWeaponPickup(in Weapon weapon) {
		if(current_weapon_ && other_weapon_) {
			if(weapon.weapon_type != current_weapon_.weapon_type && weapon.weapon_type != other_weapon_.weapon_type) {
				return 0;
			}
		}
		if(other_weapon_) {
			if(weapon.weapon_type == other_weapon_.weapon_type) {
				return other_weapon_.addAmmo() ? -1 : 0;
			}
		}
		if(current_weapon_) {
			if(weapon.weapon_type == current_weapon_.weapon_type) {
				return current_weapon_.addAmmo() ? -1 : 0;
			}
		}

		return 1;
	}

	/// <summary> -1 means we need to destroy the whole object,
	/// 0 means we don't need to do anything,
	/// 1 means we need to destroy pickup script. </summary>
	// TODO: Лучше бы optional<bool> использовал..
	public void pickWeapon(Weapon weapon) {
		if(!current_weapon_)
			current_weapon_ = weapon;
		else if(!other_weapon_)
			other_weapon_ = weapon;
		if(other_weapon_)
			other_weapon_.toggleActive(false);
		if(current_weapon_)
			current_weapon_.toggleActive(true);
	}

	public void takeDamage(float damage, Vector2 direction, float push_force) {
		player_stats_.takeDamage(damage);

		if(player_stats_.died()) {
			destroy();
		}
		velocity_ += direction * push_force;
	}

	public bool pickHealthPickup(float health) {
		return player_stats_.takeHealth(health);
	}

	public bool pickArmorPickup(float armor) {
		return player_stats_.takeArmor(armor);
	}

	public Vector2 get_rounded_position() {
		return new Vector2((int)transform.position.x, (int)transform.position.y); 
	}

	IEnumerator spawn() {
		sprite_renderer_.enabled = false;
        GameObject effect = Instantiate(spawn_effect_, transform.position, Quaternion.identity);
        yield return new WaitForSeconds(0.5f);
        Destroy(effect);
        sprite_renderer_.enabled = true;
		was_init_ = true;
		start_weapon_ = Instantiate(start_weapon_, transform.position, Quaternion.identity);
		pickWeapon(start_weapon_);
    }

	public void destroy() {
		Destroy(Instantiate(death_effect_, transform.position, Quaternion.identity), 0.5f);
		if(current_weapon_)
			Destroy(current_weapon_.gameObject);
		if(other_weapon_)
			Destroy(other_weapon_.gameObject);
		Destroy(gameObject);
	}
}