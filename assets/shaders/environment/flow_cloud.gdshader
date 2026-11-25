// Godot 4 port of Alex Strook's CC0, adapted for SkyCrew's scaled world clouds.
// "_Clouds/Clouds Unlit Vertex Color" Unity shader.
// Original source: reference/UnlitClouds/Assets/__CLOUDS/Cloud_unlit_vertex_color.shader
// Unity's distance tessellation stage has no direct Godot spatial-shader equivalent.
shader_type spatial;
render_mode unshaded, cull_back, depth_prepass_alpha;

uniform sampler2D top_texture : filter_linear_mipmap, repeat_enable;

group_uniforms world_integration;
uniform bool use_reference_space = false;
uniform bool use_world_lighting = false;
global uniform vec3 skycrew_sun_direction;
global uniform vec4 skycrew_cloud_light : source_color;
global uniform vec4 skycrew_cloud_shadow : source_color;
uniform bool use_distance_fade = false;
instance uniform float cloud_fade_start = 3000.0;
instance uniform float cloud_fade_end = 4200.0;
instance uniform float cloud_spawn_visibility = 1.0;

group_uniforms toon_lighting;
uniform float toon_strength : hint_range(0.0, 1.0, 0.01) = 0.0;
uniform float toon_band_softness : hint_range(0.01, 0.15, 0.005) = 0.045;
uniform float toon_shadow_threshold : hint_range(0.1, 0.6, 0.01) = 0.40;
uniform float toon_light_threshold : hint_range(0.5, 0.9, 0.01) = 0.72;

group_uniforms noise_a;
uniform vec3 noise_scale_a = vec3(1.0);
uniform float noise_size_a = 0.8;
uniform float speed_a = 0.5;
uniform vec3 direction_a = vec3(1.5, 0.0, 0.0);
uniform float noise_strength_a : hint_range(0.0, 1.0, 0.001) = 0.472;

group_uniforms noise_b;
uniform vec3 noise_scale_b = vec3(1.0);
uniform float noise_size_b = 1.49;
uniform float speed_b = 1.0;
uniform vec3 direction_b = vec3(1.48, -0.86, -0.36);
uniform float noise_strength_b : hint_range(0.0, 1.0, 0.001) = 0.182;

group_uniforms noise_c;
uniform vec3 noise_scale_c = vec3(1.0, 1.0, 2.02);
uniform float noise_size_c = 5.0;
uniform float speed_c = 10.36;
uniform vec3 direction_c = vec3(1.0, 0.0, 0.0);
uniform float noise_strength_c : hint_range(0.0, 1.0, 0.001) = 0.071;

group_uniforms surface_flow;
// Fragment-only controls. At 1 these reproduce the original surface sampling.
// Larger size gives larger patches; speed 0 freezes surface flow only.
uniform float flow_size : hint_range(0.1, 5.0, 0.05) = 1.0;
uniform float flow_speed : hint_range(0.0, 5.0, 0.05) = 1.0;
uniform float flow_strength : hint_range(0.0, 5.0, 0.05) = 1.0;

group_uniforms surface;
uniform float texture_detail : hint_range(0.0, 1.0, 0.001) = 1.0;
uniform vec3 texture_color : source_color = vec3(0.509434, 0.43494126, 0.5047782);
uniform vec2 tiling = vec2(0.04);
uniform float triplanar_falloff = 1.0;
uniform float vertex_color_multiplier = 1.16;
uniform vec3 rim_color : source_color = vec3(0.14150941, 0.14150941, 0.07542719);
uniform vec3 fresnel_bsp = vec3(0.41, 1.0, 3.65);
uniform bool use_procedural_color = false;
uniform vec3 procedural_shadow_color : source_color = vec3(0.17, 0.27, 0.38);
uniform vec3 procedural_light_color : source_color = vec3(0.92, 0.96, 1.0);
uniform vec3 fake_light_direction = vec3(-0.35, 0.82, 0.45);
uniform vec2 procedural_height_range = vec2(-5.0, 8.0);
uniform float procedural_displacement_strength : hint_range(0.0, 2.0, 0.01) = 0.0;
uniform float procedural_vertical_lift : hint_range(0.0, 1.0, 0.01) = 0.0;

varying vec3 cloud_world_position;
varying vec3 cloud_world_normal;
varying vec4 cloud_vertex_color;
varying vec3 cloud_reference_position;
varying vec3 cloud_reference_normal;


#include "res://assets/shaders/environment/cloud_noise_3d.gdshaderinc"


vec3 mod289_2d_3(vec3 x) {
	return x - floor(x * (1.0 / 289.0)) * 289.0;
}


vec2 mod289_2d_2(vec2 x) {
	return x - floor(x * (1.0 / 289.0)) * 289.0;
}


vec3 permute_3(vec3 x) {
	return mod289_2d_3(((x * 34.0) + 1.0) * x);
}


float simplex_noise_2d(vec2 v) {
	const vec4 c = vec4(0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439);
	vec2 i = floor(v + dot(v, c.yy));
	vec2 x0 = v - i + dot(i, c.xx);
	vec2 i1 = x0.x > x0.y ? vec2(1.0, 0.0) : vec2(0.0, 1.0);
	vec4 x12 = x0.xyxy + c.xxzz;
	x12.xy -= i1;
	i = mod289_2d_2(i);
	vec3 p = permute_3(permute_3(i.y + vec3(0.0, i1.y, 1.0)) + i.x + vec3(0.0, i1.x, 1.0));
	vec3 m = max(0.5 - vec3(dot(x0, x0), dot(x12.xy, x12.xy), dot(x12.zw, x12.zw)), vec3(0.0));
	m *= m;
	m *= m;
	vec3 x = 2.0 * fract(p * c.www) - 1.0;
	vec3 h = abs(x) - 0.5;
	vec3 ox = floor(x + 0.5);
	vec3 a0 = x - ox;
	m *= 1.79284291400159 - 0.85373472095314 * (a0 * a0 + h * h);
	vec3 gradient;
	gradient.x = a0.x * x0.x + h.x * x0.y;
	gradient.yz = a0.yz * x12.xz + h.yz * x12.yw;
	return 130.0 * dot(m, gradient);
}


vec4 triplanar_sample(sampler2D texture_map, vec3 world_position, vec3 world_normal, float falloff, vec2 texture_tiling) {
	vec3 weights = pow(abs(world_normal), vec3(max(falloff, 0.0001)));
	weights /= weights.x + weights.y + weights.z + 0.00001;
	vec3 normal_sign = sign(world_normal);
	vec4 x_projection = texture(texture_map, texture_tiling * world_position.zy * vec2(normal_sign.x, 1.0));
	vec4 y_projection = texture(texture_map, texture_tiling * world_position.xz * vec2(normal_sign.y, 1.0));
	vec4 z_projection = texture(texture_map, texture_tiling * world_position.xy * vec2(-normal_sign.z, 1.0));
	return x_projection * weights.x + y_projection * weights.y + z_projection * weights.z;
}


void vertex() {
	vec3 source_world_position = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
	// Recreate the upstream model's 1000x authoring scale and -4m pivot offset.
	// Noise and height stay attached to the shape when placed high in the world.
	vec3 source_position = source_world_position;
	float displacement_scale = 1.0;
	if (use_reference_space) {
		source_position = VERTEX * 1000.0 + vec3(0.0, -4.0, 0.0);
		displacement_scale = length(MODEL_MATRIX[0].xyz) / 1000.0;
	}
	cloud_reference_position = source_position;
	cloud_reference_normal = NORMAL;
	if (use_procedural_color && procedural_displacement_strength > 0.00001) {
		float noise_a = simplex_noise_3d(direction_a * TIME * speed_a + noise_size_a * (source_position * noise_scale_a));
		float noise_b = simplex_noise_3d(direction_b * TIME * speed_b + noise_size_b * (source_position * noise_scale_b));
		float noise_c = simplex_noise_3d(direction_c * TIME * speed_c + noise_size_c * (source_position * noise_scale_c));
		float signed_displacement = noise_a * noise_strength_a
			+ noise_b * noise_strength_b
			+ noise_c * noise_strength_c;
		float upward_billow = max(noise_a * 0.5 + noise_b * 0.28, 0.0);
		vec3 source_world_normal = normalize(MODEL_NORMAL_MATRIX * NORMAL);
		vec3 world_offset = source_world_normal * signed_displacement * procedural_displacement_strength;
		world_offset += vec3(0.0, upward_billow * procedural_vertical_lift, 0.0);
		world_offset *= displacement_scale;
		// Convert the metre-sized world offset back to local space. Using the
		// normal matrix here would apply non-uniform ellipsoid scale twice.
		VERTEX += inverse(mat3(MODEL_MATRIX)) * world_offset;
	}
	cloud_world_position = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
	cloud_world_normal = normalize(MODEL_NORMAL_MATRIX * NORMAL);
	cloud_vertex_color = COLOR;
}


void fragment() {
	float cloud_visibility = 1.0;
	if (use_distance_fade) {
		float distance_to_cloud = distance(CAMERA_POSITION_WORLD, NODE_POSITION_WORLD);
		cloud_visibility = (1.0 - smoothstep(cloud_fade_start, cloud_fade_end, distance_to_cloud))
			* cloud_spawn_visibility;
		if (cloud_visibility <= 0.001) {
			discard;
		}
	}
	vec3 world_normal = normalize(cloud_world_normal);
	// Separate the original surface animation from A/B/C vertex deformation.
	vec3 shape_position = use_reference_space ? cloud_reference_position : cloud_world_position;
	vec3 texture_normal = use_reference_space ? normalize(cloud_reference_normal) : world_normal;
	vec3 flow_position = shape_position / max(flow_size, 0.001);
	float flow_time = TIME * flow_speed;
	vec3 noise_world_position = vec3(1.0, 0.0, 0.0) * flow_time * 10.36
		+ 5.0 * (flow_position * vec3(1.0, 1.0, 2.02));
	vec4 triplanar = triplanar_sample(top_texture, noise_world_position, texture_normal, triplanar_falloff, tiling);
	float noise_a = simplex_noise_3d(vec3(1.5, 0.0, 0.0) * flow_time * 0.5
		+ 0.8 * flow_position) * 0.5 + 0.5;
	float broad_noise = simplex_noise_2d(flow_position.xz * 0.1);
	float broad_mask = mix(0.25, 1.0, broad_noise * 0.5 + 0.5);
	float texture_mask = clamp((1.0 - triplanar.r) * texture_detail * clamp(noise_a, 0.0, 1.0) * broad_mask * flow_strength, 0.0, 1.0);
	vec3 vertex_color = clamp(pow(max(cloud_vertex_color.rgb, vec3(0.0)), vec3(0.454545)) * vertex_color_multiplier, vec3(0.0), vec3(1.0));
	vec3 shadow_color = use_world_lighting ? skycrew_cloud_shadow.rgb : procedural_shadow_color;
	vec3 light_color = use_world_lighting ? skycrew_cloud_light.rgb : procedural_light_color;
	vec3 light_direction = use_world_lighting
		? normalize(vec3(skycrew_sun_direction.x, max(skycrew_sun_direction.y, 0.12), skycrew_sun_direction.z))
		: normalize(fake_light_direction);
	vec3 camera_position = INV_VIEW_MATRIX[3].xyz;
	vec3 view_direction = normalize(camera_position - cloud_world_position);
	// Keep the established backlit response and apply broad bands toward front/side light.
	float backlight = smoothstep(0.05, 0.65, -dot(view_direction, light_direction));
	float toon_mix = use_world_lighting ? toon_strength * (1.0 - backlight) : 0.0;
	if (use_procedural_color) {
		// The source FBX stores its broad lighting in vertex colors. Editable
		// spheres do not, so rebuild the same low-frequency light/shadow role.
		float height_light = smoothstep(procedural_height_range.x, procedural_height_range.y, shape_position.y);
		// Wrap the fake light around concave blend regions. A conventional
		// half-Lambert response makes every source ellipsoid reveal a dark
		// underside, which visually reconstructs the seams the union removed.
		float normal_light = smoothstep(-0.55, 0.72, dot(world_normal, light_direction));
		float light_amount = clamp(height_light * 0.42 + normal_light * 0.58, 0.0, 1.0);
		vertex_color = mix(shadow_color, light_color, light_amount);
		float shadow_band = smoothstep(toon_shadow_threshold - toon_band_softness,
			toon_shadow_threshold + toon_band_softness, light_amount);
		float light_band = smoothstep(toon_light_threshold - toon_band_softness,
			toon_light_threshold + toon_band_softness, light_amount);
		float band_amount = 0.12 + 0.42 * shadow_band + 0.46 * light_band;
		vec3 band_color = mix(shadow_color, min(light_color, vec3(1.0)), band_amount);
		vertex_color = mix(vertex_color, band_color, toon_mix);
	}
	vec3 texture_tint = use_world_lighting ? mix(shadow_color, light_color, 0.13) : texture_color;
	texture_mask *= 1.0 - toon_mix * 0.85;
	vec3 base_color = mix(vertex_color, texture_tint, texture_mask);
	float fresnel = fresnel_bsp.x + fresnel_bsp.y * pow(1.0 - clamp(dot(world_normal, view_direction), 0.0, 1.0), fresnel_bsp.z);
	vec3 rim = rim_color;
	if (use_world_lighting) {
		rim *= light_color * (0.45 + 0.55 * max(dot(world_normal, light_direction), 0.0));
	}
	ALBEDO = clamp(base_color + fresnel * rim, vec3(0.0), vec3(1.0));
	ALPHA = cloud_visibility;
	ROUGHNESS = 1.0;
}
