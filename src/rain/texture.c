#include <rain/texture.h>

#define STB_IMAGE_IMPLEMENTATION
#include <stb_image.h>

void rain_texture_from_file(
	struct rain_texture *restrict this,
	const char *restrict path,
	enum rain_texture_format format,
	sg_usage usage
) {
	[[maybe_unused]] int channels;
	this->usage = usage;
	stbi_set_flip_vertically_on_load(1);
	stbi_uc *data = stbi_load(path, &this->width, &this->height, &channels, format);
	if (!data) {
		fprintf(stderr, "texture/ERR failed to load texture at '%s': %s\n",
			path, stbi_failure_reason());
	}
	// fprintf(stderr, "texture/INFO %dx%d @ %p\n", this->width, this->height, data);

	sg_pixel_format pixel_format_map[] = {
		0,
		SG_PIXELFORMAT_R32F,
		SG_PIXELFORMAT_RG16F,
		SG_PIXELFORMAT_RGBA8, // the values won't be packed, so the alpha will be 0?
		SG_PIXELFORMAT_RGBA8,
	};

	// channels instead of format because it might be 0.
	// (so stbi uses whatever is in the image)
	this->format = pixel_format_map[channels];

	this->image = sg_make_image(&(sg_image_desc){
		.data.subimage[0][0] = {
			.ptr = data,
			.size = this->width * this->height * channels
		},
		.width = this->width,
		.height = this->height,
		.type = SG_IMAGETYPE_2D,
		.num_slices = 1,
		.pixel_format = this->format,
		.num_mipmaps = 1,
		.usage = usage
	});

	stbi_image_free(data);
	this->exists = true;
}

void rain_texture_destroy(struct rain_texture *this) {
	sg_destroy_image(this->image);
	this->exists = false;
}
