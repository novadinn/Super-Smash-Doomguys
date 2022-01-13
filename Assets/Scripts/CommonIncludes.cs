using UnityEngine;

public static class CommonIncludes {
    public static LayerMask team_a_mask = 1 << 7;
    public static LayerMask team_b_mask = 1 << 8;
    public static LayerMask players_masks = team_a_mask | team_b_mask;

    public static LayerMask obstacles_layer = 1 << 6;
    public static LayerMask team_a_collision_masks = obstacles_layer.value | 1 << 8;
    public static LayerMask team_b_collision_masks = obstacles_layer.value | 1 << 7;
    
    public static LayerMask all_masks = obstacles_layer | players_masks;

    public static LayerMask get_layer_from_team(Team team) {
        return team == Team.TEAM_A ? team_a_collision_masks : team_b_collision_masks;
    }
}
