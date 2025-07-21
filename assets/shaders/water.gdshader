shader_type spatial;
render_mode blend_mix, cull_disabled, depth_draw_opaque;

uniform sampler2D water_tex;
uniform float alpha_val : hint_range(0.0, 1.0) = 0.6;
uniform vec2 uv_scale = vec2(1.0, 1.0);
uniform float move_speed = 1.0;
uniform float move_amount = 0.05;

varying vec2 world_uv;

void vertex() {
    vec3 world_pos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
    world_uv = world_pos.xz * uv_scale;
}

void fragment() {
    float offset = sin(TIME * move_speed) * move_amount;
    vec2 moved_uv = world_uv + vec2(offset, 0.0);

    vec4 tex = texture(water_tex, fract(moved_uv));
    ALBEDO = tex.rgb;
    ALPHA = alpha_val;
}