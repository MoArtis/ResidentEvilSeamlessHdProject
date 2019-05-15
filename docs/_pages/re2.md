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
share: true
discord: true

gallery_seamless:
  - url: /img/galleries/seamless/Seam_crop_f.jpg
    image_path: /img/galleries/seamless/Seam_crop_th.jpg
    alt: "Filtering applied on the mask texture"
    title: "Filtering applied on the mask texture"
  - url: /img/galleries/seamless/Mixed_crop_f.jpg
    image_path: /img/galleries/seamless/Mixed_crop_th.jpg
    alt: "Basic solution: Skipping the mask textures"
    title: "Basic solution: Skipping the mask textures"
  - url: /img/galleries/seamless/Seamless_crop_f.jpg
    image_path: /img/galleries/seamless/Seamless_crop_th.jpg
    alt: "The RESHDP solution"
    title: "The RESHDP solution"

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
header:
  og_image: "/img/RE2SHDP_logo_og.jpg"
  hide_title: true
  overlay_image: /img/re2header_uw.jpg
  actions:
   - label: "Download"
     icon: /img/gamecube_logo_rect.png
     caption: "RE2SHDP v1.0 + <a href=\"#customized-dolphin-build\">Custom Dolphin</a><br>Mirror links: <a href=\"https://drive.google.com/file/d/1x6UOL7yoBd11hIs5llPYLJfX43C0PWSQ/view?usp=sharing\">Google Drive</a> - <a href=\"/assets/RE2SHDP-1.0 + Custom Dolphin.torrent\">Torrent</a>"
     url: "https://mega.nz/#!OB4GSaQA!ysGTBlVle2SqTAmQuSpXU8Jca0lND4HXvBODs7d0_TE"
  #  - label: "Download"
  #    fa_icon: "fas fa-images"
  #    caption: "RE2SHDP v1.0 PNG Pack"
  #    url: "https://mega.nz/#!0sNFCYbY!3raWMmiajJpmu8zFPC6F_ZsNDQv-u0uRgVpcq98NJeI"
---

<!-- <div class="feature__wrapper"> -->

Time to re-experience this classic survival-horror game with **Machine Learning** upscaled backgrounds,<br> **vectorized masks** and many other small improvements in this **all-in-one texture pack.**<br>
{% include fa n="fas fa-exclamation-circle" %} Please note that **RESHDP** is a free fan project. {% include fa n="fas fa-exclamation-circle" %}
{: .text-center}
{: .notice }

<!-- </div> -->

# How the backgrounds look like?

{% include carousel_juxtapose.html name="backgrounds" width=960 images=page.backgrounds prev_label="Original incl. masks" next_label="RE2SHDP" comp_suffix="_altMaskSource" img_format=".jpg" selectedIndex=0 %}

<div class="feature__wrapper"></div>

# What changes does this pack include?

{% include feature_gallery.html id="gallery_do" %}

<div class="feature__wrapper"></div>

# What's the big deal with "seamless masks"?

The RESHDP texture pack solves the **seam issue** that always comes up when any kind of filtering or upscaling is applied to the game's **mask textures**. 

{% include feature_gallery.html id="gallery_seamless" %}

Our solution was to create a tool that analyzes the game data to **regenerate completely new mask textures** from the upscaled background textures. The original mask textures are not used or processed in any way.

<div class="feature__wrapper"></div>

# Frequently Asked Questions

>**How do I install the pack?**<br>
Our pack comes along a custom version of Dolphin (a Gamecube emulator). All you have to do is to extract the Zip file, open Dolphin.exe, select the folder containing your game ISO, configure the controller if needed and play! **We don't provide the game ISO** and we will not help you to find one. If you can't open the zip file, you might need to download and use [7Zip <i class="fas fa-external-link-alt"></i>](https://www.7-zip.org/download.html).
{: .notice }

>**Is it compatible with the PAL version?**<br>
Yes but the pack was made with the US (NTSC) version in mind. Therefore you need to rename the folder "\*dolphin_path\*\User\Load\Textures\\<wbr>**GHAE08**" to "**GHAP08**". Please note that the texts and some 3d models will not be upscaled. We plan to make the pack fully compatible with the PAL version in the future.
{: .notice }

>**Can I use the pack on the PC version ?**<br>
Sadly no... Two community patches exist for RE2 PC: _Peixoto_ and _Classic Rebirth_. _Peixoto_ would require us to dump the textures of the game manually one by one. _Classic Rebirth_, while being a great and easy to use patch, doesn't feature texture upscaling (yet). We will work on a PC version when it will be possible.
{: .notice }

>**...and on Dolphin for Mac/Android?**<br>
Yes. But we don't provide a custom version of Dolphin for these platforms. We are looking into it though.
{: .notice }

<!-- >**Do I need to download the 2 packs ?**<br>
No, if you just want to play, only the first link is necessary ("Custom Dolphin + RE2SHDP"). The second link is only provided to people who want to have a look at the source files since the playable pack is using optimized DDS files.
{: .notice } -->

>**What can I do if I have hiccups?**<br>
If you are experiencing **noticeable performance hiccups and slowdowns** when the background changes or when you open the inventory, try this on Dolphin: Open the "Graphics" menu, go to the "Advanced" tab and, in the "Utility" section, turn on the the "**Prefetch Custom Textures**" option. You need to restart the game and wait for a bit for the change to take effect. 
{: .notice }

<!-- Please note that this option uses a lot of RAM (~4GB), the game will slow down a lot for a minute and turning on and off the pack -->

>**How to turn off the pack while playing?**<br>
You can turn the pack on and off while playing with **ALT + F10**. Please note that if you have the "**Prefetch Custom Textures**" option activated, the textures will have to be reloaded entirely again.
{: .notice }

>**Can you help me regarding Dolphin?**<br>
We only made tiny modifications to the emulator so, if you have any problem or question regarding Dolphin you better off checking their [FAQ <i class="fas fa-external-link-alt"></i>](https://dolphin-emu.org/docs/faq/) or asking on their [forum <i class="fas fa-external-link-alt"></i>](https://forums.dolphin-emu.org/).
{: .notice }

>**How can I contact you?**<br>
You can chat with us on [Discord <i class="fab fa-discord"></i>](https://discord.gg/HCZWV6q) or write us an issue on [Github Issues <i class="fab fa-github"></i>](https://github.com/MoArtis/ResidentEvilSeamlessHdProject/issues). We also have a <a title="The team" href="#top" class="team__toggle" type="button" style="margin-right: 0em">Team page <i class="fas fa-users"></i></a> with different ways to contact us.
{: .notice }

<div class="feature__wrapper"></div>

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
**Neural networks upscaling is not magic.** The algorithm has an especially hard time with dark areas and RE2 is clearly not a game with bright and colorful backgrounds. Expect to see a lot of **"melting" artefacts** on dark corners and distant parts of the backgrounds.<br>
Small texts will also end up being processed as melting garbage. We replaced them when the result was too distracting.

![image-right](/img/perfect/mask_issue.jpg){: .align-right}
Many original **mask textures don't line up perfectly** with their respective background texture. Thus some pixels which are not part of the foreground appear on top of the 3D models.<br>
These issues are barely noticeable at such low resolution and on a CRT (And the game was intended to be displayed on a CRT like any game of that era).<br>
But these issues can be very distracting at an high resolution and on a flatscreen. We touched up the worst offenders but thousands of man-hours is required to clean up everything.

![image-left](/img/perfect/vectorization_issue.jpg){: .align-left}
The full process to create this pack is quite complex and involve multiple tools through multiple steps: Game data analysis, PC to GameCube texture matching, analysis of mask special cases, mask alpha layers vectorization, texture upscaling, texture recreation... **Bugs are to be expected** with such process. So even if we carefully tested the pack during development, you will certainly encounter small unexpected issues.

That being said, if you encounter such issues, have any problem with the pack or notice a big imperfection, don't hesitate to report it on [Github Issues <i class="fab fa-github"></i>](https://github.com/MoArtis/ResidentEvilSeamlessHdProject/issues).

Your feedbacks will help us to improve the quality of the pack.

<div class="feature__wrapper"></div>

# Video showcase

{% include video id="VF6-hbhFHGI" provider="youtube" %}

<div class="feature__wrapper"></div>

<!-- # Comparison Gallery

{% include carousel_juxtapose.html name="backgrounds" width=960 images=page.backgrounds prev_label="Original incl. masks" next_label="RE2SHDP" comp_suffix="_altMaskSource" img_format=".jpg" selectedIndex=10 %}

<div class="feature__wrapper"></div> -->