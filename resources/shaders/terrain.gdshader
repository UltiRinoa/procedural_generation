shader_type spatial;

uniform sampler2D main_texture : source_color, filter_nearest;

void vertex() {
}

void fragment() {
	vec4 color = texture(main_texture, UV);
	ALBEDO = color.rgb;
}
