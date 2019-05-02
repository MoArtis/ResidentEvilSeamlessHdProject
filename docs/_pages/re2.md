---
title: "RE2SHDP"
layout: showcase
permalink: /re2/
show_logo: true
show_title: false
use_juxtapose: true
use_carousel: true
github_issue: https://github.com/MoArtis/ResidentEvilSeamlessHdProject/issues
# redirect_from: "/"
share: "true"

gallery_dolphin:
  - url: /img/galleries/dolphin/aspect_ratio_f.jpg
    image_path: /img/galleries/dolphin/aspect_ratio_th.jpg
    alt: "Dolphin Aspect ratio"
    title: "New aspect ratio mode: Fit 4:3"
  - url: /img/galleries/dolphin/music_bug_f.jpg
    image_path: /img/galleries/dolphin/music_bug_th.jpg
    alt: "Dolphin Music bug fix"
    title: "No music stuttering (This scene was impacted)"
  - url: /img/galleries/dolphin/portable_f.jpg
    image_path: /img/galleries/dolphin/portable_th.jpg
    alt: "Dolphin Portable"
    title: "Pre-configured and Portable"

gallery_do:
  - url: /img/galleries/do/bgs_masks_re2shdp_f.jpg
    image_path: /img/galleries/do/bgs_masks_re2shdp_th.jpg
    alt: "RE2SHDP Bgs and Masks"
    title: "Upscaled backgrounds with seamless masks"
  - url: /img/galleries/do/bg_texts_re2shdp_f.jpg
    image_path: /img/galleries/do/bg_texts_re2shdp_th.jpg
    alt: "RE2SHDP Bg Texts"
    title: "Restored integrated texts"
  - url: /img/galleries/do/3dmodels_re2shdp_f.jpg
    image_path: /img/galleries/do/3dmodels_re2shdp_th.jpg
    alt: "RE2SHDP 3d Model textures"
    title: "Upscaled 3D model textures"
  - url: /img/galleries/do/computer_re2shdp_f.jpg
    image_path: /img/galleries/do/computer_re2shdp_th.jpg
    alt: "RE2SHDP Computer"
    title: "Manually edited in-game screens"
  - url: /img/galleries/do/file_re2shdp_f.jpg
    image_path: /img/galleries/do/file_re2shdp_th.jpg
    alt: "RE2SHDP file"
    title: "Recreated Image-based 'files'"
  - url: /img/galleries/do/inventory_portrait_re2shdp_f.jpg
    image_path: /img/galleries/do/inventory_portrait_re2shdp_th.jpg
    alt: "RE2SHDP inventory and portraits"
    title: "Upscaled menu elements and HR portraits."

backgrounds:
  - /bgs/ROOM_60E_01
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
  - /bgs/ROOM_60F_04

#Header
#excerpt: " "
header:
  og_image: "/img/RE2SHDP_logo_og.jpg"
  hide_title: true
  #min-height: 25em
  # image: /img/re2header.jpg
  overlay_image: /img/re2header_uw.jpg
  #caption: ""
  actions:
   - label: "Download"
    #  fa_icon: download
     icon: /img/gamecube_logo_rect.png
     caption: "Custom Dolphin + RE2SHDP v1.0 DDS files <a href=\"#customized-dolphin-build\">more info...</a>"
     url: "https://mega.nz/#!OB4GSaQA!ysGTBlVle2SqTAmQuSpXU8Jca0lND4HXvBODs7d0_TE"
  #  - label: "Download"
  #    fa_icon: desktop
  #    caption: "RE2SHDP v1.0-Gigapixel-1x PNG files for Classic Rebirth <a href=\"\">more info...</a>"
  #    url: ""
   - label: "Download"
     fa_icon: images
     caption: "RE2SHDP v1.0 PNG files"
     url: "https://mega.nz/#!0sNFCYbY!3raWMmiajJpmu8zFPC6F_ZsNDQv-u0uRgVpcq98NJeI"
---

<!-- <div class="feature__wrapper"> -->

Time to re-experience this classic survival-horror game with **Neural-networks** processed backgrounds,<br> **seamless** masks and many other small improvements in this **all-in-one texture pack.**<br>
{% include fa n="exclamation-circle" %} Please note that **RESHDP** is a free fan project. {% include fa n="exclamation-circle" %}
{: .text-center}
{: .notice--primary}

<!-- </div> -->

# How the backgrounds look like?

{% include carousel_juxtapose.html name="backgrounds" width=960 images=page.backgrounds prev_label="Original incl. masks" next_label="RE2SHDP" comp_suffix="_altMaskSource" img_format=".jpg" selectedIndex=0 %}

<div class="feature__wrapper"></div>

# What changes does this pack include?

{% include feature_gallery.html id="gallery_do" %}

<div class="feature__wrapper"></div>

<!-- # What's the big deal with "seamless masks"?

While it's not the first time the backgrounds of the classic RE games are being upscaled with ESRGAN or other similar tools, the result is usually not playable.

Trying to just use upscaled background will give you that result:

<div class="align-center" style="width:960px; margin-bottom:2em">
{% include juxtapose.html name="sl_mixed" prev_img="/seamless/Mixed.jpg" next_img="/seamless/Seamless.jpg" prev_label="Bg Only" next_label="Seamless" %}
</div>

Parts of the background is still pixelated. This is due to the fact that Classic RE games use pre-rendered backgrounds covered by sprites to create an illusion of depth. The parts still pixelated on the previous was these mask sprites being drawn on top of the background.

The mask setup in these games is quite complex. With multiple layer of "mask groups" and special cases where groups are switched on and off to animate the background.

The masks are packed as a texture sheet and like a puzzle, each parts (usually) correspond to a spot on the background texture.

Thus, processing the mask texture sheets will always end up creating seams around these masks. The color of a mask parts will be mixed with other parts and will miss the color information where there is transparency.

Our solution is to use the game data to determine proper Mask to background texture mapping coordinates, allowing us to regenerate the masks from any processed and upscaled background textures.

comparison with a directly upscaled and leave it as such (pixel)

<div class="feature__wrapper"></div> -->

# Customized Dolphin build

Since 2016, RE2 and RE3 on Dolphin, a great open-source Gamecube emulator, suffer from a [music stuttering bug <i class="fas fa-external-link-alt"></i>](https://bugs.dolphin-emu.org/issues/9840). 

To ensure the best experience, we created a custom build by modifying a recent version of the emulator. This allows us to distribute our pack using the [BC7 texture format <i class="fas fa-external-link-alt"></i>](https://docs.microsoft.com/en-us/windows/desktop/direct3d11/bc7-format) ensuring no additional stuttering when the background changes.

Here are the most important modifications:

{% include feature_gallery.html id="gallery_dolphin" %}

As the version is portable, you can use it alongside another install of Dolphin. Its "Users" folder is located next to its executable.

The source code of this custom build is available on [Github <i class="fab fa-github"></i>](https://github.com/MoArtis/dolphin)

<div class="feature__wrapper"></div>

# Is it perfect?

No. And we would like to give you more details about the most obvious issues.

![image-left](/img/perfect/text_issue.jpg){: .align-left}
Neural networks upscaling is not magic. The algorithm has an especially hard time with dark areas and RE2 is clearly not a game with bright and colorful backgrounds. Expect to see a lot of "melting" artefacts on dark corners and distant parts of the backgrounds.<br>
Small texts will also end up being processed as melting garbage. We replaced them when the result was too distracting.

![image-right](/img/perfect/mask_issue.jpg){: .align-right}
Many original mask textures don't line up perfectly with their respective background texture. Thus some pixels which are not part of the foreground appear on top of the 3D models.<br>
These issues are barely noticeable at such low resolution and on a CRT (And the game was intended to be displayed on a CRT like any game of that era).<br>
But these issues can be very distracting at an high resolution and on a flatscreen. We touched up the worst offenders but thousands of man-hours is required to clean up everything.

![image-left](/img/perfect/vectorization_issue.jpg){: .align-left}
The full process to create this pack is quite complex and involve multiple tools through multiple steps: Game data analysis, PC to GameCube texture matching, analysis of mask special cases, mask alpha layers vectorization, texture upscaling, texture recreation... Bugs are expected with such complicated process. So even if we carefully tested the pack during development, you will certainly encounter small unexpected issues.

That being said, if you encounter such issues, have any problem with the pack or notice a big imperfection, don't hesitate to report it on [Github Issues <i class="fab fa-github"></i>](https://github.com/MoArtis/ResidentEvilSeamlessHdProject/issues).

Your feedbacks will help us to improve the quality of the pack.

<div class="feature__wrapper"></div>

# Video showcase

{% include video id="VF6-hbhFHGI" provider="youtube" %}

<div class="feature__wrapper"></div>

<!-- # Comparison Gallery

{% include carousel_juxtapose.html name="backgrounds" width=960 images=page.backgrounds prev_label="Original incl. masks" next_label="RE2SHDP" comp_suffix="_altMaskSource" img_format=".jpg" selectedIndex=10 %}

<div class="feature__wrapper"></div> -->