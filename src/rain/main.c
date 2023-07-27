#include <rain/window.h>
#include <rain/camera.h>
#include <rain/transform.h>
#include <rain/renderer.h>

#include <mono/jit/jit.h>
#include <mono/metadata/assembly.h>
#include <mono/metadata/debug-helpers.h>
#include <mono/metadata/mono-config.h>

#include "engine.h"
#include "interop.h"

struct rain_engine rain__engine_;


int main() {
	rain_window_init(&rain__engine_.window, "Mokosh (Engine)", 800, 600);

	// { // initialize camera with default parameters.
	// 	struct rain_camera *cam = &rain__engine_.default_camera;
	// 	rain_camera_set_position(cam, HMM_V3(0.0f, 0.0f, -2.0f));
	// 	rain_camera_set_rotation(cam, HMM_Q(0.0f, 0.0f, 0.0f, 1.0f));
	// 	rain_camera_set_aspect(cam, 800.0f / 600.0f);
	// 	rain_camera_set_type(cam, RAIN_CAMERA_PROJECTION_ORTHOGRAPHIC);
	// 	rain_camera_set_near(cam, 0.01f);
	// 	rain_camera_set_far(cam, 10.0f);
	// 	rain_camera_set_vert_size(cam, 5.0f);
	// }

	rain_renderer_init(&rain__engine_.renderer, &rain__engine_.window);

	// struct rain_transform transform;
	// rain_transform_set_position(&transform, HMM_V3(0.0f, 0.0f, 0.0f));
	// rain_transform_set_rotation(&transform, HMM_Q(0.0f, 0.0f, 0.0f, 1.0f));
	// rain_transform_set_scale(&transform, HMM_V3(1.0f, 1.0f, 1.0f));

	mono_config_parse(nullptr);

	MonoDomain *domain = mono_jit_init("RainEngine_Domain");

	if (!domain) {
		fprintf(stderr, "Failed to initialize Rain Engine Domain\n");
		return 1;
	}

	MonoAssembly *assembly = mono_domain_assembly_open(domain, "build/csrain/Test.dll");
	if (!assembly) {
		fprintf(stderr, "Failed to open C# Assembly @ '%s'\n", "build/csrain/Test.dll");
		return 1;
	}

	rain_bind_mono_interop();
	
	MonoImage *image = mono_assembly_get_image(assembly);
	if (!image) {
		fprintf(stderr, "Failed to get C# Assembly Image\n");
		return 1;
	}
	
	MonoMethodDesc *script_entry_method_desc = mono_method_desc_new("RainEngine.Main:Entry()", true);
	MonoMethod *script_entry_method = mono_method_desc_search_in_image(script_entry_method_desc, image);
	if (script_entry_method) {
		mono_runtime_invoke(script_entry_method, nullptr, (void*[0]){}, NULL);
	}
	
	MonoMethodDesc *script_render_method_desc = mono_method_desc_new("RainEngine.Main:Render()", true);
	MonoMethod *script_render_method = mono_method_desc_search_in_image(script_render_method_desc, image);
	
	MonoMethodDesc *script_update_method_desc = mono_method_desc_new("RainEngine.Main:Update", true);
	MonoMethod *script_update_method = mono_method_desc_search_in_image(script_update_method_desc, image);

	float lastTime = rain_window_get_time(&rain__engine_.window);
	while (!rain_window_should_close(&rain__engine_.window)) {
		float currentTime = rain_window_get_time(&rain__engine_.window);
		float deltaTime = currentTime - lastTime;

		rain_renderer_begin_render(&rain__engine_.renderer);

		if (script_update_method) {
			mono_runtime_invoke(script_update_method, nullptr, (void*[1]){&(float){deltaTime}}, NULL);
		}
			
		if (script_render_method) {
			mono_runtime_invoke(script_render_method, nullptr, (void*[0]){}, NULL);
		}
	
		rain_renderer_end_render(&rain__engine_.renderer);
		rain_window_frame(&rain__engine_.window);
		
		lastTime = currentTime;
	}

	mono_jit_cleanup(domain);
	domain = nullptr;

	rain_renderer_deinit(&rain__engine_.renderer);

	rain_window_deinit(&rain__engine_.window);
}
