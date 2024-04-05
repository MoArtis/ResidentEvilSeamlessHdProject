---
title: "RE1SHDP"
layout: showcase
permalink: /re1/
show_logo: true
show_title: false
use_juxtapose: true
use_carousel: true
github_issue: https://github.com/MoArtis/ResidentEvilSeamlessHdProject/issues
# redirect_from: "/"
share: true
discord: true

gallery_seamless:
  - url: /img/galleries/RE2_seamless/Seam_crop_f.jpg
    image_path: /img/galleries/RE2_seamless/Seam_crop_th.jpg
    alt: "Filtering applied on the mask texture"
    title: "Filtering applied on the mask texture"
  - url: /img/galleries/RE2_seamless/Mixed_crop_f.jpg
    image_path: /img/galleries/RE2_seamless/Mixed_crop_th.jpg
    alt: "Basic solution: Skipping the mask textures"
    title: "Basic solution: Skipping the mask textures"
  - url: /img/galleries/RE2_seamless/Seamless_crop_f.jpg
    image_path: /img/galleries/RE2_seamless/Seamless_crop_th.jpg
    alt: "The RESHDP solution"
    title: "The RESHDP solution"

gallery_dolphin:
  - url: /img/galleries/RE2_dolphin/aspect_ratio_f.jpg
    image_path: /img/galleries/RE2_dolphin/aspect_ratio_th.jpg
    alt: "Dolphin Aspect ratio"
    title: "New aspect ratio mode: Fit 4:3"
  - url: /img/galleries/RE2_dolphin/music_bug_f.jpg
    image_path: /img/galleries/RE2_dolphin/music_bug_th.jpg
    alt: "Dolphin Music bug fix"
    title: "No music stuttering (This scene was impacted)"
  - url: /img/galleries/RE2_dolphin/portable_f.jpg
    image_path: /img/galleries/RE2_dolphin/portable_th.jpg
    alt: "Dolphin Portable"
    title: "Pre-configured and Portable"

gallery_do:
  - url: /img/galleries/RE1_do/bgs_masks_re1shdp_f.jpg
    image_path: /img/galleries/RE1_do/bgs_masks_re1shdp_th.jpg
    alt: "RE1SHDP Bgs and Masks"
    title: "Upscaled backgrounds with vectorized masks"
  - url: /img/galleries/RE1_do/bg_texts_re1shdp_f.jpg
    image_path: /img/galleries/RE1_do/bg_texts_re1shdp_th.jpg
    alt: "RE1SHDP Bg Texts"
    title: "Restored integrated images and texts"
  - url: /img/galleries/RE1_do/3dmodels_re1shdp_f.jpg
    image_path: /img/galleries/RE1_do/3dmodels_re1shdp_th.jpg
    alt: "RE1SHDP 3d Model textures"
    title: "Upscaled 3D model textures"
  - url: /img/galleries/RE1_do/computer_re1shdp_f.jpg
    image_path: /img/galleries/RE1_do/computer_re1shdp_th.jpg
    alt: "RE1SHDP Computer"
    title: "Manually edited in-game screens"
  - url: /img/galleries/RE1_do/vfx_re1shdp_f.jpg
    image_path: /img/galleries/RE1_do/vfx_re1shdp_th.jpg
    alt: "RE1SHDP visual effects"
    title: "Improved visual effects"
  - url: /img/galleries/RE1_do/inventory_portrait_re1shdp_f.jpg
    image_path: /img/galleries/RE1_do/inventory_portrait_re1shdp_th.jpg
    alt: "RE1SHDP inventory and portraits"
    title: "Upscaled menu elements and HQ portraits."

fmvs:
  - /RE1_fmvs/0
  - /RE1_fmvs/1
  - /RE1_fmvs/2
  - /RE1_fmvs/3
  - /RE1_fmvs/4

backgrounds:
  - /RE1_bgs/ROOM_109_03
  - /RE1_bgs/ROOM_10B_02
  - /RE1_bgs/ROOM_20E_02
  - /RE1_bgs/ROOM_100_00
  - /RE1_bgs/ROOM_104_01
  - /RE1_bgs/ROOM_105_04
  - /RE1_bgs/ROOM_107_00
  - /RE1_bgs/ROOM_115_02
  - /RE1_bgs/ROOM_203_03
  - /RE1_bgs/ROOM_302_01
  - /RE1_bgs/ROOM_400_00
  - /RE1_bgs/ROOM_507_00
  - /RE1_bgs/ROOM_513_02

downloads:
   - caption: "1.0 Pack for Classic Rebirth"
     buttons:
      - label: "ModDb"
        url: "https://www.moddb.com/mods/resident-evil-seamless-hd-project"

#Header
header:
  # min-height: "600px"
  og_image: "/img/RE1SHDP_logo_og.jpg"
  hide_title: true
  overlay_image: /img/re1header_uw.jpg
  actions:
   - caption: ""
     buttons:
      - label: "Downloads"
        url: "#downloads"
  #  - label: "Download"
  #    fa_icon: "fas fa-images"
  #    caption: "RE2SHDP v1.0 PNG Pack"
  #    url: "https://mega.nz/#!0sNFCYbY!3raWMmiajJpmu8zFPC6F_ZsNDQv-u0uRgVpcq98NJeI"
---

<!-- <div class="feature__wrapper"> -->

Time to re-experience this classic survival-horror game with **Machine Learning** upscaled backgrounds,<br> **seamless masks** and many other small improvements in this **all-in-one texture pack.**<br>
{% include fa n="fas fa-exclamation-circle" %} Please note that **RESHDP** is a free fan project. {% include fa n="fas fa-exclamation-circle" %}<br>
Please check the [FAQ](#frequently-asked-questions) before playing.
{: .text-center}
{: .notice }

# Downloads
{% include downloads.html downloads=page.downloads %}

<div class="feature__wrapper"></div>

# Installation guide

{% include video id="7DJ8TqxiWDo" provider="youtube" %}

<div class="feature__wrapper"></div>

# How the backgrounds look like?

{% include carousel_juxtapose.html name="backgrounds" width=1000 images=page.backgrounds prev_label="RE1 PC" next_label="RE1SHDP" comp_suffix="-1" img_format=".jpg" selectedIndex=0 %}

<div class="feature__wrapper"></div>

# What changes does this pack include?

{% include feature_gallery.html id="gallery_do" %}

<div class="feature__wrapper"></div>

# FMV

Our RE1 pack directly includes upscaled FMVs. No need to install an additional pack this time. 

On top of the regular videos, It features an optional Capcom logo, an alternative version of Jill's shark tank video and a fully redone credits sequence.

{% include carousel_juxtapose.html name="fmvs" width=1000 images=page.fmvs prev_label="RE1 PC" next_label="RE1SHDP" comp_suffix="_og" img_format=".jpg" selectedIndex=0 %}

<div class="feature__wrapper"></div>

# Video showcase

{% include video id="-nqH73D_nhg" provider="youtube" %}

<div class="feature__wrapper"></div>

# Frequently Asked Questions

>**How do I install the pack?**<br>
Instructions are provided in the description of the ModDB files. The Mediakite version is mandatory for the Classic Rebirth patche. More info can be found here: [Classic Rebirth](https://classicrebirth.com/index.php/downloads/resident-evil-classic-rebirth/).
<br>Just remember: GAMES ARE NOT INCLUDED FOR OBVIOUS LEGAL RESONS.
{: .notice }


>**Why is it so complicated to install the PC pack?**<br>
It involves two other parties who worked hard to make the PC version playable on recent OS and get HD textures working. We can't include everything in a pre-package because it wouldn't be fair for the original creators. When the PC versions will be truly natively compatible, it will be easier.
{: .notice }


>**How can I contact you?**<br>
You can chat with us on [Discord <i class="fab fa-discord"></i>](https://discord.gg/td9tVqygB4) or write us an issue on [Github <i class="fab fa-github"></i>](https://github.com/MoArtis/ResidentEvilSeamlessHdProject/issues).   If you are part of the Dolphin community, you can check out [our dedicated forum thread <i class="fas fa-external-link-alt"></i>](https://forums.dolphin-emu.org/Thread-resident-evil-2-resident-evil-3-hd-ui-and-upscaled-textures).  We also have a <a title="The team" href="#top" class="team__toggle" type="button" style="margin-right: 0em">Team page <i class="fas fa-users"></i></a> with even more ways to contact us.
{: .notice }


<div class="feature__wrapper"></div>


# What's the big deal with "seamless masks"?

The RESHDP texture pack solves the **seam issue** that always comes up when any kind of filtering or upscaling is applied to the game's **mask textures**. 

{% include feature_gallery.html id="gallery_seamless" %}

Our solution was to create a tool that analyzes the game data to **regenerate completely new mask textures** from the upscaled background textures. The original mask textures are not used or processed in any way.

<div class="feature__wrapper"></div>


# Is it perfect?

No. And we would like to give you more details about the most obvious issues.

![image-left]({{ '/img/RE2_perfect/text_issue.jpg' | relative_url }}){: .align-left}
**Neural networks upscaling is not magic.** The algorithm has an especially hard time with dark areas and RE games are clearly not games with the brightest and the most colorful backgrounds. Expect to see a lot of **"melting" artefacts** on dark corners and distant parts of the backgrounds.<br>
Small texts will also end up being processed as melting garbage. We replaced them when the result was too distracting.

![image-right]({{ '/img/RE2_perfect/mask_issue.jpg' | relative_url }}){: .align-right}
Many original **mask textures don't line up perfectly** with their respective background texture. Thus some pixels which are not part of the foreground appear on top of the 3D models.<br>
These issues are barely noticeable at such low resolution and on a CRT (And the game was intended to be displayed on a CRT like any game of that era).<br>
But these issues can be very distracting at an high resolution and on a flatscreen. We touched up the worst offenders but thousands of man-hours is required to clean up everything.

![image-left]({{ '/img/RE2_perfect/vectorization_issue.jpg' | relative_url }}){: .align-left}
The full process to create this pack is quite complex and involve multiple tools through multiple steps: Game data analysis, PC to GameCube texture matching, analysis of mask special cases, mask alpha layers vectorization, texture upscaling, texture recreation... **Bugs are to be expected** with such process. So even if we carefully tested the pack during development, you will certainly encounter small unexpected issues.

That being said, if you encounter such issues, have any problem with the pack or notice a big imperfection, don't hesitate to report it on [Github Issues <i class="fab fa-github"></i>](https://github.com/MoArtis/ResidentEvilSeamlessHdProject/issues).

Your feedbacks will help us to improve the quality of the pack.

<div class="feature__wrapper"></div>

