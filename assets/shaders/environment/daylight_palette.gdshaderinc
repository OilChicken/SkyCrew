global uniform vec3 skycrew_sun_direction;

float daylight(float sun_height) {
	return smoothstep(-0.10, 0.22, sun_height);
}

float twilight(float sun_height) {
	return smoothstep(-0.22, -0.06, sun_height) * (1.0 - smoothstep(0.07, 0.32, sun_height));
}

vec3 sky_gradient(vec3 direction, vec3 sun_direction) {
	float day = daylight(sun_direction.y);
	float dusk = twilight(sun_direction.y);
	float up = pow(clamp(direction.y, 0.0, 1.0), 0.46);
	float down = smoothstep(0.0, 0.65, -direction.y);
	float toward_sun = pow(max(0.0, dot(direction, normalize(vec3(sun_direction.x, 0.01, sun_direction.z)))), 3.0);
	float horizon_band = exp(-abs(direction.y) * 5.0);
	vec3 zenith = mix(vec3(0.006, 0.010, 0.032), vec3(0.10, 0.35, 0.66), day);
	vec3 horizon = mix(vec3(0.025, 0.038, 0.075), vec3(0.61, 0.78, 0.85), day);
	horizon = mix(horizon, mix(vec3(0.27, 0.23, 0.38), vec3(0.98, 0.43, 0.18), toward_sun),
		dusk * (0.45 + 0.50 * toward_sun));
	zenith = mix(zenith, vec3(0.09, 0.075, 0.20), dusk * 0.65);
	vec3 sky = mix(horizon, zenith, up);
	sky += vec3(0.22, 0.07, 0.025) * dusk * toward_sun * horizon_band;
	vec3 nadir = mix(vec3(0.012, 0.021, 0.05), vec3(0.37, 0.57, 0.68), day);
	return mix(sky, nadir, down);
}
