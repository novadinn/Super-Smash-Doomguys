using UnityEngine;

public class PowerupPickup : Pickup {
    enum PowerupType {
        HEALTH_PACK,
        ARMOR_PACK
    }

    [SerializeField] PowerupType type_;
    [SerializeField] float powerup_amount_;

    protected override void reactToCollision(RaycastHit2D hit) {
        if(type_ == PowerupType.HEALTH_PACK) {
            if(hit.collider.GetComponent<Player>().pickHealthPickup(powerup_amount_)) {
                Destroy(gameObject);
            }
        } else if(type_ == PowerupType.ARMOR_PACK) {
            if(hit.collider.GetComponent<Player>().pickArmorPickup(powerup_amount_)) {
                Destroy(gameObject);
            }
        }
    }
}

