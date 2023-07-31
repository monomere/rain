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
	rain_window_init(&rain__engine_.window, "Mokosh (Engine)", 1920/1.5, 1080/1.5);
	rain_renderer_init(&rain__engine_.renderer, &rain__engine_.window);

	mono_config_parse(nullptr);

	MonoDomain *domain = mono_jit_init("RainEngine_Domain");

	if (!domain) {
		fprintf(stderr, "Failed to initialize Rain Engine Domain\n");
		return 1;
	}

	const char *csout = "src/csrain/bin/Debug/net4.6.2/csrain.dll";
	MonoAssembly *assembly = mono_domain_assembly_open(domain, csout);
	if (!assembly) {
		fprintf(stderr, "Failed to open C# Assembly @ '%s'\n", csout);
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
		mono_runtime_invoke(script_entry_method, nullptr, (void*[0]){}, nullptr);
	}
	
	MonoMethodDesc *script_render_method_desc = mono_method_desc_new("RainEngine.Main:Render()", true);
	MonoMethod *script_render_method = mono_method_desc_search_in_image(script_render_method_desc, image);
	
	MonoMethodDesc *script_update_method_desc = mono_method_desc_new("RainEngine.Main:Update", true);
	MonoMethod *script_update_method = mono_method_desc_search_in_image(script_update_method_desc, image);

	MonoMethodDesc *script_destroy_method_desc = mono_method_desc_new("RainEngine.Main:Destroy()", true);
	MonoMethod *script_destroy_method = mono_method_desc_search_in_image(script_destroy_method_desc, image);
	
	float lastTime = rain_window_get_time(&rain__engine_.window);
	while (!rain_window_should_close(&rain__engine_.window)) {
		float currentTime = rain_window_get_time(&rain__engine_.window);
		rain__engine_.delta_time = currentTime - lastTime;

		if (script_update_method) {
			mono_runtime_invoke(script_update_method, nullptr,
				(void*[1]){&(float){rain__engine_.delta_time}}, nullptr);
		}
		
		rain_renderer_begin_render(&rain__engine_.renderer);
		if (script_render_method) {
			mono_runtime_invoke(script_render_method, nullptr,
				(void*[0]){}, nullptr);
		}
		rain_renderer_end_render(&rain__engine_.renderer);
	
		rain_window_frame(&rain__engine_.window);
		
		lastTime = currentTime;
	}

	if (script_destroy_method) {
		mono_runtime_invoke(script_destroy_method, nullptr, (void*[0]){}, nullptr);
	}

	mono_jit_cleanup(domain);
	domain = nullptr;

	rain_renderer_deinit(&rain__engine_.renderer);
	rain_window_deinit(&rain__engine_.window);
}
