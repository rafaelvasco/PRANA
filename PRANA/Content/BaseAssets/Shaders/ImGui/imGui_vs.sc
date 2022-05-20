$input a_position, a_texcoord0, a_color0
$output v_texcoord0, v_color0

#include <bgfx_shader.sh>

void main() 
{
	v_texcoord0 = a_texcoord0;
	v_color0 = a_color0;
	
	vec2 pos = a_position.xy;
	gl_Position = mul(u_modelViewProj, vec4(pos.x, pos.y, 0.0, 1.0));	
}

