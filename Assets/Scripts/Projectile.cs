using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {
    const float kDestroyTime = 5;

    const float kExplodeRadius = 2f;

    enum CollisionEffect {
        DEFAULT,
        EXPLODE
    }
    [SerializeField] CollisionEffect effect_;

    [SerializeField] GameObject hit_effect_;

    [SerializeField] float damage_;
    [SerializeField] float move_speed_;
    [SerializeField] float push_force_;

    LayerMask collision_layer_;

    void Start() {
        StartCoroutine(startDestroy());
    }

    public Projectile init(LayerMask layer) {
        collision_layer_ = layer;
        return this;
    }
    
    void Update() { // TODO: fix that mess
        float move_distance = move_speed_ * Time.deltaTime;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right, move_distance, collision_layer_);
        if(hit) {
            transform.position = hit.point;
            switch(effect_) {
                case CollisionEffect.DEFAULT:
                Player player = hit.collider.GetComponent<Player>();
                if(player) {
                    player.takeDamage(damage_, transform.right, push_force_);
                }
                break;
                case CollisionEffect.EXPLODE:
                RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, kExplodeRadius, Vector2.zero, 1, collision_layer_);
                foreach(RaycastHit2D hit2D in hits) {
                    Player collided_player = hit2D.collider.GetComponent<Player>();
                    if(collided_player) {
                        collided_player.takeDamage(damage_*(1/Vector2.Distance(collided_player.transform.position, hit.point)), transform.right, push_force_);
                    }
                }
                LayerMask layer = collision_layer_ == CommonIncludes.team_a_collision_masks ? CommonIncludes.team_a_mask : CommonIncludes.team_b_mask;
                RaycastHit2D owner_hit = Physics2D.CircleCast(transform.position, kExplodeRadius, Vector2.zero, 1, layer);
                if(owner_hit) {
                    Player owner = owner_hit.collider.GetComponent<Player>();
                    if(owner) {
                        owner.takeDamage(damage_*(1/Vector2.Distance(owner.transform.position, hit.point)), -transform.right, push_force_);
                    }
                }
                break;
            }
            destroy();
        } else {
            transform.Translate(transform.right * move_distance, Space.World);
        }
    }

    IEnumerator startDestroy() {
        yield return new WaitForSeconds(kDestroyTime);
        destroy();
    }

    void destroy() {
        Destroy(Instantiate(hit_effect_, transform.position, transform.rotation), 0.417f);
        Destroy(gameObject);
    }
}
