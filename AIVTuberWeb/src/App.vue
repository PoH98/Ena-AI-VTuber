<template>
  <canvas ref="canvas"></canvas>
</template>

<script>
import { Application } from '@pixi/app';
import { Ticker, TickerPlugin } from '@pixi/ticker';
import { Live2DModel } from 'pixi-live2d-display/cubism4';
import { InteractionManager } from '@pixi/interaction';
import { Renderer } from '@pixi/core';
export default {
  name: 'App',
  data() {
    return {
      app: null,
      model: null,
      audioCtx: new AudioContext(),
      processor: null
    }
  },
  async mounted() {
    Live2DModel.registerTicker(Ticker);
    Application.registerPlugin(TickerPlugin);
    Renderer.registerPlugin('interaction', InteractionManager);
    this.app = new Application({
      view: this.$refs.canvas,
      resizeTo: window,
      backgroundColor: 0x00FF00
    });
    this.model = await Live2DModel.from('/maidmade2/maidmade2.model3.json');
    this.app.stage.addChild(this.model);

    // transforms
    this.model.x = 100;
    this.model.y = 100;
    this.model.rotation = Math.PI;
    this.model.skew.x = Math.PI;
    this.model.scale.set(0.2, 0.2);
    this.model.anchor.set(1, 0.1);
    this.model._autoInteract = false;
    let vm = this;
    window.model = this.model;
    let i = 0;
    this.processor = vm.audioCtx.createScriptProcessor(2048, 1, 1);
    this.processor.onaudioprocess = function (evt) {
      var input = evt.inputBuffer.getChannelData(0)
        , len = input.length
        , total = i = 0
        , rms;
      while (i < len) total += Math.abs(input[i++]);
      rms = Math.sqrt(total / len);
      let params = window.model.internalModel.settings.getLipSyncParameters();
      for (let i = 0; i < Object.keys(params).length; i++) {
        console.log(rms / 100);
        window.model.internalModel.coreModel.setParameterValueById(params[i], rms, 1);
      }
    };
    window.playAudio = function (audio) {
      window.playing = true;
      audio.addEventListener('canplaythrough', async function () {
        let source = vm.audioCtx.createMediaElementSource(audio);
        source.connect(vm.processor);
        source.connect(vm.audioCtx.destination);
        vm.processor.connect(vm.audioCtx.destination);
        await new Promise((resolve) => {
          audio.onended = resolve;
          audio.play();
        });
        window.played.onPlayed();
      }, false);
    }
  }
}
</script>

<style>
body {
  margin: 0;
}

#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
}
</style>
