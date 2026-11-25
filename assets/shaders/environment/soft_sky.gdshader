shader_type sky;

#include "res://assets/shaders/environment/daylight_palette.gdshaderinc"

uniform float sun_radius : hint_range(0.3, 2.5, 0.05) = 0.85;
uniform float halo_strength : hint_range(0.0, 2.0, 0.05) = 0.7;
uniform float sun_intensity : hint_range(1.0, 24.0, 0.5) = 10.0;
uniform float star_intensity : hint_range(0.0, 4.0, 0.1) = 0.85;

vec3 hash_star(vec2 cell) {
	return fract(sin(vec3(dot(cell, vec2(127.1, 311.7)),
		dot(cell, vec2(269.5, 183.3)), dot(cell, vec2(419.2, 371.9)))) * 43758.5453);
}

vec3 stars(vec3 direction) {
	vec2 uv = vec2(atan(direction.z, direction.x) / TAU + 0.5, asin(clamp(direction.y, -1.0, 1.0)) / PI + 0.5);
	vec2 grid = uv * vec2(360.0, 180.0);
	vec2 cell = floor(grid);
	vec3 seed = hash_star(cell);
	vec2 offset = fract(grid) - (0.18 + 0.64 * seed.xy);
	// Compensate the equirectangular map's stretching toward the poles.
	offset.x *= max(0.1, cos((uv.y - 0.5) * PI));
	float radius = mix(0.012, 0.065, pow(seed.x, 5.0));
	float aa = max(0.012, length(fwidth(grid)) * 0.32);
	float point = 1.0 - smoothstep(radius, radius + aa, length(offset));
	float selected = step(0.963, seed.z);
	float twinkle = 0.85 + 0.15 * sin(TIME * (0.5 + seed.x) + seed.y * TAU);
	vec3 tint = mix(vec3(0.60, 0.74, 1.0), vec3(1.0, 0.88, 0.68), seed.y);
	return tint * point * selected * twinkle * (0.12 + 2.2 * pow(seed.x, 6.0));
}

void sky() {
	vec3 sun = normalize(skycrew_sun_direction);
	COLOR = sky_gradient(EYEDIR, sun);
	if (!AT_CUBEMAP_PASS) {
		float separation = length(EYEDIR - sun);
		float radius = 2.0 * sin(radians(sun_radius) * 0.5);
		float horizon_clip = smoothstep(-0.006, 0.002, EYEDIR.y);
		float visible = smoothstep(-0.045, 0.015, sun.y);
		float warmth = 1.0 - smoothstep(0.0, 0.35, sun.y);
		vec3 core = mix(vec3(1.0, 0.96, 0.80), vec3(1.0, 0.48, 0.12), warmth);
		float disc = 1.0 - smoothstep(radius * 0.94, radius * 1.06, separation);
		float corona = exp(-separation / max(radius, 0.001) * 1.4);
		float aureole = exp(-pow(separation * 7.0, 1.5));
		float scatter = exp(-pow(separation * 2.5, 1.8));
		COLOR += core * halo_strength * (0.35 * aureole + 0.12 * scatter) * visible;
		COLOR += core * (disc * sun_intensity * mix(1.0, 0.35, warmth) + corona * 1.8) * visible * horizon_clip;

		float night = 1.0 - smoothstep(-0.25, -0.07, sun.y);
		float above_horizon = smoothstep(0.015, 0.20, EYEDIR.y);
		if (night > 0.0) {
			vec3 star_color = stars(EYEDIR);
			float milky_band = exp(-abs(dot(EYEDIR, normalize(vec3(0.30, 0.82, -0.48)))) * 18.0);
			COLOR += (star_color * star_intensity + vec3(0.006, 0.009, 0.017) * milky_band) * night * above_horizon;
			float moon_distance = length(EYEDIR + sun);
			float moon = 1.0 - smoothstep(0.014, 0.016, moon_distance);
			float moon_halo = exp(-moon_distance * 14.0) * 0.045;
			COLOR += vec3(0.34, 0.46, 0.68) * (moon + moon_halo) * night * horizon_clip;
		}
	}
}
