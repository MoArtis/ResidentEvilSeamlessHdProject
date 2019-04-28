---
title: "RE2SHDP"
layout: showcase
permalink: /re2/
show_logo: true
show_title: false
use_juxtapose: true
use_carousel: true

gallery_menus:
  - url: /img/galleries/menus/bgs_masks_re2shdp_f.jpg
    image_path: /img/galleries/menus/bgs_masks_re2shdp_th.jpg
    alt: "RE2SHDP Bgs and Masks"
    title: "Upscaled backgrounds with seamless masks"
  - url: /img/galleries/menus/bg_texts_re2shdp_f.jpg
    image_path: /img/galleries/menus/bg_texts_re2shdp_th.jpg
    alt: "RE2SHDP Bg Texts"
    title: "Restored integrated texts"
  - url: /img/galleries/menus/3dmodels_re2shdp_f.jpg
    image_path: /img/galleries/menus/3dmodels_re2shdp_th.jpg
    alt: "RE2SHDP 3d Model textures"
    title: "Upscaled 3D model textures"
  - url: /img/galleries/menus/computer_re2shdp.jpg
    image_path: /img/galleries/menus/computer_re2shdp_th.jpg
    alt: "RE2SHDP Computer"
    title: "Manually edited in-game screens"
  - url: /img/galleries/menus/file_re2shdp.jpg
    image_path: /img/galleries/menus/file_re2shdp_th.jpg
    alt: "RE2SHDP file"
    title: "Recreated Image-based 'files'"
  - url: /img/galleries/menus/inventory_portrait_re2shdp.jpg
    image_path: /img/galleries/menus/inventory_portrait_re2shdp_th.jpg
    alt: "RE2SHDP inventory and portraits"
    title: "Upscaled menu elements and HR portraits."

backgrounds:
  - /bgs/ROOM_109_03
  - /bgs/ROOM_118_05
  - /bgs/ROOM_11B_03
  - /bgs/ROOM_200_06
  - /bgs/ROOM_20B_05
  - /bgs/ROOM_300_01
  - /bgs/ROOM_501_12
  - /bgs/ROOM_508_11
  - /bgs/ROOM_603_10
  - /bgs/ROOM_60D_02
  - /bgs/ROOM_60E_01
  - /bgs/ROOM_60F_04

#Header
#excerpt: " "
header:
  hide_title: true
  #min-height: 25em
  # image: /img/re2header.jpg
  overlay_image: /img/re2header_uw.jpg
  #caption: "Photo credit: [**Unsplash**](https://unsplash.com)"
  actions:
   - label: "Download"
    #  fa_icon: download
     icon: /img/gamecube_logo_rect.png
     caption: "Custom Dolphin + RE2SHDP v1.0 DDS files <a href=\"#customized-dolphin-build\">more info...</a>"
     url: "https://boards.4channel.org/v/"
  #  - label: "Download"
  #    fa_icon: desktop
  #    caption: "RE2SHDP v1.0-Gigapixel-1x PNG files for Classic Rebirth <a href=\"\">more info...</a>"
  #    url: "https://boards.4channel.org/v/"
   - label: "Download"
     fa_icon: images
     caption: "RE2SHDP v1.0 PNG files"
     url: "https://boards.4channel.org/v/"
---

<!-- <div class="feature__wrapper"> -->

Time to re-experience this classic survival-horror game with **Neural-networks** processed backgrounds,<br> **seamless** mask textures and many other small improvements in one pack.<br>
:warning: Please note that **RESHDP** is a free fan project. :warning:
{: .text-center}
{: .notice--primary}

<!-- </div> -->

# How the backgrounds look like?

    {% include carousel_juxtapose.html name="backgrounds" width=960 images=page.backgrounds prev_label="Original incl. masks" next_label="RE2SHDP" comp_suffix="_altMaskSource" img_format=".jpg" selectedIndex=10 %}

<div class="feature__wrapper"></div>

# What changes does this pack include?

{% include feature_gallery.html id="gallery_menus" %}

<div class="feature__wrapper"></div>

# What's the big deal with "seamless masks"?

If you pay enough attention you will notice that the existing "HD texture packs" for the classic Resident Evil games suffer from seams around the masks or just use native resolution textures for the masks.

This is due to the fact that Classic RE games use pre-rendered backgrounds covered by sprites to create an illusion of depth.

Their setup is quite complex, with multiple layer of "mask groups" and special cases where groups are switched on and off to animate the background.

The masks are packed as a texture sheet and like a puzzle, each parts (usually) correspond to a spot on the background texture.

Thus processing the mask texture sheets will always end up creating seams around these masks. The color of a mask parts will be mixed with other parts and will miss the color information where there is transparency.

Our solution is to use the game data to determine proper Mask to background texture mapping coordinates, allowing us to regenerate the masks from any processed and upscaled background textures.

comparison with a directly upscaled and leave it as such (pixel)

<div class="feature__wrapper"></div>

# Customized Dolphin build

* 3:4 screen ratio
* Music bug workaround
* Pre configured and portable

<div class="feature__wrapper"></div>

# Is it perfect?

No. 

* Neural networks upscaling is not magic. can generate big mess especially with such a dark game. Small texts usually end up as garbage. We replaced them as much as possible.
* Many masks are imperfect. Barely noticeable at such low resolution and on a CRT. But can be distracting as HR on a flatscreen. We touched up the worst offenders but that would require years of works to clean up everything.
* The full process is quite complex: Game data analysis, PC to GameCube texture matching, Mask special cases analysis, Mask alpha layer vertorization, Base Texture upscaling, Texture recreation... Bugs might went through our careful test process. So don't hesitate to report bugs on Github.
* noticeable edits. 

Mask bugs vectorization

Something can be better? you spotted a bug? help us by filling up a new issue on Github => link

<div class="feature__wrapper"></div>

# Video showcase

  {% include video id="VF6-hbhFHGI" provider="youtube" %}

<div class="feature__wrapper"></div>

# Comparison Gallery

  {% include carousel_juxtapose.html name="backgrounds" width=960 images=page.backgrounds prev_label="Original incl. masks" next_label="RE2SHDP" comp_suffix="_altMaskSource" img_format=".jpg" selectedIndex=10 %}

<div class="feature__wrapper"></div>