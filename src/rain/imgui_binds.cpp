#include <imgui.h>
#include <stdio.h>
#include "imgui_binds.h"
#define GLFW_INCLUDE_NONE
#include <GLFW/glfw3.h>
#include <sokol_gfx.h>
#include <imgui_impl_glfw.h>

extern "C" {
#include "engine.h"
}

static struct rain_imgui {
	sg_bindings bind;
	sg_pipeline pipeline;
	sg_shader shader;
	size_t max_vertices;
} im_;

struct rain_imgui_ub {
	ImVec2 disp_size;
};

extern "C" {

void rain_imgui_init(size_t max_vertices, struct rain_imgui_data *data) {
	im_.max_vertices = max_vertices;
	ImGui::CreateContext();
	ImGui::StyleColorsDark();
	ImGuiIO &io = ImGui::GetIO();
	io.ConfigFlags |= ImGuiConfigFlags_DockingEnable;
	io.IniFilename = "data/imgui.ini";
	io.Fonts->AddFontDefault();
	
	// Do something about this being specific
	// to GLFW. Should this be in `window`? nah.
	ImGui_ImplGlfw_InitForOther(
		*(GLFWwindow**)rain__engine_.window.handle,
		true
	);

	// dynamic vertex- and index-buffers for imgui-generated geometry
	sg_buffer_desc vbuf_desc = { };
	vbuf_desc.usage = SG_USAGE_STREAM;
	vbuf_desc.size = im_.max_vertices * sizeof(ImDrawVert);
	im_.bind.vertex_buffers[0] = sg_make_buffer(&vbuf_desc);

	sg_buffer_desc ibuf_desc = { };
	ibuf_desc.type = SG_BUFFERTYPE_INDEXBUFFER;
	ibuf_desc.usage = SG_USAGE_STREAM;
	ibuf_desc.size = im_.max_vertices * 3 * sizeof(ImDrawIdx);
	im_.bind.index_buffer = sg_make_buffer(&ibuf_desc);

	// font texture and sampler for imgui's default font
	unsigned char *font_pixels;
	int font_width, font_height;
	io.Fonts->GetTexDataAsRGBA32(&font_pixels, &font_width, &font_height);

	sg_image_desc img_desc = { };
	img_desc.width = font_width;
	img_desc.height = font_height;
	img_desc.pixel_format = SG_PIXELFORMAT_RGBA8;
	img_desc.data.subimage[0][0] = sg_range{ font_pixels, size_t(font_width * font_height * 4) };
	sg_image font_image = sg_make_image(&img_desc);

	io.Fonts->SetTexID((void*)(uintptr_t)font_image.id);

	sg_sampler_desc smp_desc = { };
	smp_desc.wrap_u = SG_WRAP_CLAMP_TO_EDGE;
	smp_desc.wrap_v = SG_WRAP_CLAMP_TO_EDGE;
	im_.bind.fs.samplers[0] = sg_make_sampler(&smp_desc);

	// shader object for imgui rendering
	sg_shader_desc shd_desc = { };
	auto &ub = shd_desc.vs.uniform_blocks[0];
	ub.size = sizeof(rain_imgui_ub);
	ub.uniforms[0].name = "disp_size";
	ub.uniforms[0].type = SG_UNIFORMTYPE_FLOAT2;
	shd_desc.vs.source =
			"#version 330\n"
			"uniform vec2 disp_size;\n"
			"layout(location=0) in vec2 position;\n"
			"layout(location=1) in vec2 texcoord0;\n"
			"layout(location=2) in vec4 color0;\n"
			"out vec2 uv;\n"
			"out vec4 color;\n"
			"void main() {\n"
			"    gl_Position = vec4(((position/disp_size)-0.5)*vec2(2.0,-2.0), 0.5, 1.0);\n"
			"    uv = texcoord0;\n"
			"    color = color0;\n"
			"}\n";
	shd_desc.fs.images[0].used = true;
	shd_desc.fs.samplers[0].used = true;
	shd_desc.fs.image_sampler_pairs[0].used = true;
	shd_desc.fs.image_sampler_pairs[0].glsl_name = "tex";
	shd_desc.fs.image_sampler_pairs[0].image_slot = 0;
	shd_desc.fs.image_sampler_pairs[0].sampler_slot = 0;
	shd_desc.fs.source =
			"#version 330\n"
			"uniform sampler2D tex;\n"
			"in vec2 uv;\n"
			"in vec4 color;\n"
			"out vec4 frag_color;\n"
			"void main() {\n"
			"    frag_color = texture(tex, uv) * color;\n"
			"}\n";
	im_.shader = sg_make_shader(&shd_desc);

	// pipeline object for imgui rendering
	sg_pipeline_desc pip_desc = { };
	pip_desc.layout.buffers[0].stride = sizeof(ImDrawVert);
	auto &attrs = pip_desc.layout.attrs;
	attrs[0].format = SG_VERTEXFORMAT_FLOAT2;
	attrs[1].format = SG_VERTEXFORMAT_FLOAT2;
	attrs[2].format = SG_VERTEXFORMAT_UBYTE4N;
	pip_desc.shader = im_.shader;
	pip_desc.index_type = SG_INDEXTYPE_UINT16;
	pip_desc.colors[0].blend.enabled = true;
	pip_desc.colors[0].blend.src_factor_rgb = SG_BLENDFACTOR_SRC_ALPHA;
	pip_desc.colors[0].blend.dst_factor_rgb = SG_BLENDFACTOR_ONE_MINUS_SRC_ALPHA;
	pip_desc.colors[0].write_mask = SG_COLORMASK_RGB;
	im_.pipeline = sg_make_pipeline(&pip_desc);

	data->context = ImGui::GetCurrentContext();
	ImGui::GetAllocatorFunctions(
		(ImGuiMemAllocFunc *)&data->alloc_func,
		(ImGuiMemFreeFunc *)&data->free_func,
		&data->user_ptr
	);

	fprintf(stderr, "ImGui version: %s\n", ImGui::GetVersion());
}

void rain_imgui_deinit() {
	sg_destroy_pipeline(im_.pipeline);
	sg_destroy_buffer(im_.bind.index_buffer);
	sg_destroy_buffer(im_.bind.vertex_buffers[0]);
	sg_destroy_buffer(im_.bind.vertex_buffers[0]);
	sg_destroy_shader(im_.shader);
	sg_destroy_image(im_.bind.fs.images[0]);
	sg_destroy_sampler(im_.bind.fs.samplers[0]);
	ImGui_ImplGlfw_Shutdown();
	ImGui::DestroyContext();
}

void rain_imgui_begin_render() {
	int width, height;
	rain_window_get_fb_size(&rain__engine_.window, &width, &height);
	
	ImGuiIO &io = ImGui::GetIO();
	io.DisplaySize = ImVec2(float(width), float(height));
	io.DeltaTime = (float) rain__engine_.delta_time;
	ImGui::NewFrame();
	ImGui::DockSpaceOverViewport();
}

static void imgui_draw_(ImDrawData *draw_data);

void rain_imgui_end_render() {
	ImGui::Render();
	imgui_draw_(ImGui::GetDrawData());
	int width, height;
	rain_window_get_fb_size(&rain__engine_.window, &width, &height);
	sg_apply_scissor_rect(0, 0, width, height, true);
}

bool rain_imgui_begin(
	const char *name,
	bool *is_open
) {
	return ImGui::Begin(name, is_open);
}

bool rain_imgui_button(const char *label) {
	return ImGui::Button(label);
}

void rain_imgui_label(const char *label) {
	ImGui::Text("%s", label);
}

void rain_imgui_image(
	const struct rain_texture *texture,
	float width, float height
) {
	ImGui::Image((void*)(uintptr_t)texture->image.id, { width, height },
		{ 0, 1 }, { 1, 0 });
}

#define INPUT_FUNC(NAME, TO, TYPE, ...) \
	void rain_imgui_##NAME(const char *label, TYPE *value) { TO(label, __VA_ARGS__); }
#define INPUT_FUNC2(NAME, TO, TYPE, ...) \
	void rain_imgui_##NAME(const char *label, TYPE *value, float min, float max) \
	{ TO(label, __VA_ARGS__); }

INPUT_FUNC(drag_float, ImGui::DragFloat, float, value);
INPUT_FUNC(drag_float2, ImGui::DragFloat2, rain_float2, &value->x);
INPUT_FUNC(drag_float3, ImGui::DragFloat3, rain_float3, &value->x);
INPUT_FUNC(drag_float4, ImGui::DragFloat4, rain_float4, &value->x);
INPUT_FUNC(input_float, ImGui::InputFloat, float, value);
INPUT_FUNC(input_float2, ImGui::InputFloat2, rain_float2, &value->x);
INPUT_FUNC(input_float3, ImGui::InputFloat3, rain_float3, &value->x);
INPUT_FUNC(input_float4, ImGui::InputFloat4, rain_float4, &value->x);
INPUT_FUNC2(slider_float, ImGui::SliderFloat, float, value, min, max);
INPUT_FUNC2(slider_float2, ImGui::SliderFloat2, rain_float2, &value->x, min, max);
INPUT_FUNC2(slider_float3, ImGui::SliderFloat3, rain_float3, &value->x, min, max);
INPUT_FUNC2(slider_float4, ImGui::SliderFloat4, rain_float4, &value->x, min, max);

bool rain_imgui_tree_node(void *id, bool *selected, const char *label) {
	int flags
		= ImGuiTreeNodeFlags_OpenOnArrow
		| ImGuiTreeNodeFlags_OpenOnDoubleClick
		;
	if (selected) flags |= ImGuiTreeNodeFlags_Selected;
	return ImGui::TreeNodeEx(id, flags, "%s", label);
}

void rain_imgui_tree_pop() {
	ImGui::TreePop();
}

bool rain_imgui_is_item_clicked() {
	return ImGui::IsItemClicked();
}

void rain_imgui_input_text(const char *label, char *buf, size_t bufsize) {
	ImGui::InputText(label, buf, bufsize);
}

void rain_imgui_demo() {
	ImGui::ShowDemoWindow();
}

void rain_imgui_end() {
	ImGui::End();
}

static void imgui_draw_(ImDrawData *draw_data) {
	assert(draw_data);
	if (draw_data->CmdListsCount == 0) {
		return;
	}

	// render the command list
	sg_apply_pipeline(im_.pipeline);
	rain_imgui_ub vs_params;
	vs_params.disp_size.x = ImGui::GetIO().DisplaySize.x;
	vs_params.disp_size.y = ImGui::GetIO().DisplaySize.y;
	sg_apply_uniforms(SG_SHADERSTAGE_VS, 0, SG_RANGE(vs_params));
	for (int cl_index = 0; cl_index < draw_data->CmdListsCount; cl_index++) {
		const ImDrawList* cl = draw_data->CmdLists[cl_index];

		// append vertices and indices to buffers, record start offsets in resource binding struct
		const uint32_t vtx_size = cl->VtxBuffer.size() * sizeof(ImDrawVert);
		const uint32_t idx_size = cl->IdxBuffer.size() * sizeof(ImDrawIdx);
		const uint32_t vb_offset = sg_append_buffer(im_.bind.vertex_buffers[0], { &cl->VtxBuffer.front(), vtx_size });
		const uint32_t ib_offset = sg_append_buffer(im_.bind.index_buffer, { &cl->IdxBuffer.front(), idx_size });
		/* don't render anything if the buffer is in overflow state (this is also
				checked internally in sokol_gfx, draw calls that attempt from
				overflowed buffers will be silently dropped)
		*/
		if (sg_query_buffer_overflow(im_.bind.vertex_buffers[0]) ||
				sg_query_buffer_overflow(im_.bind.index_buffer))
		{
			continue;
		}

		im_.bind.vertex_buffer_offsets[0] = vb_offset;
		im_.bind.index_buffer_offset = ib_offset;

		int base_element = 0;
		sg_image last_image = { SG_INVALID_ID };
		for (const ImDrawCmd &pcmd : cl->CmdBuffer) {
			if (pcmd.UserCallback) {
				pcmd.UserCallback(cl, &pcmd);
			} else {
				sg_image img = { (uint32_t)(uintptr_t)pcmd.GetTexID() };
				if (img.id != last_image.id) {
					last_image = im_.bind.fs.images[0] = img;
					sg_apply_bindings(&im_.bind);
				}
				const int scissor_x = int(pcmd.ClipRect.x);
				const int scissor_y = int(pcmd.ClipRect.y);
				const int scissor_w = int(pcmd.ClipRect.z - pcmd.ClipRect.x);
				const int scissor_h = int(pcmd.ClipRect.w - pcmd.ClipRect.y);
				sg_apply_scissor_rect(scissor_x, scissor_y, scissor_w, scissor_h, true);
				sg_draw(base_element, pcmd.ElemCount, 1);
			}
			base_element += pcmd.ElemCount;
		}
	}
}

}

