Shader "Custom/SimpleShader" {
	SubShader {
    // draw after all opaque objects (queue = 2001):
    Tags { "Queue"="Geometry+2" }
    Pass {
      // keep the image behind it
    }
  } 
}
