using UnityEngine;
using System;

public class Weapon : MonoBehaviour {

    const float kDestroyTime = 1;

    const float kNailgunSuperAttackSpeed = 0.5f;

    public enum WeaponType {
        NAILGUN,
        ROCKET_LAUNCHER,
        RAILGUN,
        SHOTGUN,
        GRANADE_LAUNCHER
    }
    public WeaponType weapon_type;

    [SerializeField] Projectile projectile_prefab_;
    [SerializeField] Transform[] muzzle_transforms_;

    [SerializeField] GameObject destroy_effect_;

    SpriteRenderer renderer_;

    [SerializeField] float attack_delay_;
    Timer delay_between_attacks_timer_;
    Timer destroy_timer_ = null;

    [SerializeField] int max_ammo_;
    int ammo_;
    float look_angle_;

    void Awake() {
        renderer_ = GetComponent<SpriteRenderer>();
        delay_between_attacks_timer_ = new Timer(attack_delay_, true);
        destroy_timer_ = new Timer(kDestroyTime);

        ammo_ = max_ammo_/2;
    }

    void Update() {
        delay_between_attacks_timer_.update(Time.deltaTime);
    }
    
    public void setTransform(Vector2 position, Vector2 direction) {
        transform.position = position;

        if(direction != Vector2.zero) {
            look_angle_ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.eulerAngles = new Vector3(0,0,look_angle_);
        }

        if(look_angle_ < 89 && look_angle_ > -89) {
            transform.localScale = new Vector3(transform.localScale.x, 1, transform.localScale.z);
        } else {
            transform.localScale = new Vector3(transform.localScale.x, -1, transform.localScale.z);
        }
    }

    public void toggleActive(bool toggle) {
        renderer_.enabled = toggle;
    }

    public void attack(Team team) {
        if(!delay_between_attacks_timer_.active() && ammo_ > 0) {
            foreach(Transform muzzle_transform in muzzle_transforms_) {
                Instantiate(projectile_prefab_, muzzle_transform.position, muzzle_transform.rotation).init(CommonIncludes.get_layer_from_team(team));
                delay_between_attacks_timer_.reset();
            }
            ammo_ -= 1;
        }
    }

    public void addAmmo() {
        ammo_ = Mathf.Min(ammo_ + max_ammo_/3, max_ammo_);
    }

    public void startDestroy() {
        destroy_timer_.update(Time.deltaTime);
        if(!destroy_timer_.active()) {
            Destroy(Instantiate(destroy_effect_, transform.position, Quaternion.identity), 0.417f);
            Destroy(gameObject);
        }
    }

    public void stopDestroy() {
        destroy_timer_.reset();
    }
}
