(function(){"use strict";var n={4660:function(n,e,t){var o=t(9242),r=t(3396);const i={ref:"canvas"},a=(0,r._)("div",{class:"text",id:"text"},null,-1);function l(n,e,t,o,l,s){return(0,r.wg)(),(0,r.iD)(r.HY,null,[(0,r._)("canvas",i,null,512),a],64)}var s=t(9815),u=t(6565),c=t(7063),d=t(6024),f=t(9096),h={name:"App",data(){return{app:null,model:null,audioCtx:new AudioContext,processor:null}},async mounted(){c._Y.registerTicker(u.vB),s.M.registerPlugin(u.Sb),f.Th.registerPlugin("interaction",d.bx),this.app=new s.M({view:this.$refs.canvas,resizeTo:window,backgroundColor:65280}),this.model=await c._Y.from("/maidmade2/maidmade2.model3.json"),this.app.stage.addChild(this.model),this.model.x=100,this.model.y=100,this.model.rotation=Math.PI,this.model.skew.x=Math.PI,this.model.scale.set(.2,.2),this.model.anchor.set(1,.1),this.model._autoInteract=!1;let n=this;window.model=this.model;let e=0;this.processor=n.audioCtx.createScriptProcessor(2048,1,1),this.processor.onaudioprocess=function(n){var t,o=n.inputBuffer.getChannelData(0),r=o.length,i=e=0;while(e<r)i+=Math.abs(o[e++]);t=Math.sqrt(i/r);let a=window.model.internalModel.settings.getLipSyncParameters();for(let e=0;e<Object.keys(a).length;e++)console.log(t/100),window.model.internalModel.coreModel.setParameterValueById(a[e],t,1)},window.playAudio=function(e){window.playing=!0,e.addEventListener("canplaythrough",(async function(){let t=n.audioCtx.createMediaElementSource(e);t.connect(n.processor),t.connect(n.audioCtx.destination),n.processor.connect(n.audioCtx.destination),await new Promise((n=>{e.onended=n,e.play()})),window.played.onPlayed()}),!1)}}},p=t(89);const m=(0,p.Z)(h,[["render",l]]);var w=m;(0,o.ri)(w).mount("#app")}},e={};function t(o){var r=e[o];if(void 0!==r)return r.exports;var i=e[o]={id:o,loaded:!1,exports:{}};return n[o].call(i.exports,i,i.exports,t),i.loaded=!0,i.exports}t.m=n,function(){var n=[];t.O=function(e,o,r,i){if(!o){var a=1/0;for(c=0;c<n.length;c++){o=n[c][0],r=n[c][1],i=n[c][2];for(var l=!0,s=0;s<o.length;s++)(!1&i||a>=i)&&Object.keys(t.O).every((function(n){return t.O[n](o[s])}))?o.splice(s--,1):(l=!1,i<a&&(a=i));if(l){n.splice(c--,1);var u=r();void 0!==u&&(e=u)}}return e}i=i||0;for(var c=n.length;c>0&&n[c-1][2]>i;c--)n[c]=n[c-1];n[c]=[o,r,i]}}(),function(){t.n=function(n){var e=n&&n.__esModule?function(){return n["default"]}:function(){return n};return t.d(e,{a:e}),e}}(),function(){t.d=function(n,e){for(var o in e)t.o(e,o)&&!t.o(n,o)&&Object.defineProperty(n,o,{enumerable:!0,get:e[o]})}}(),function(){t.g=function(){if("object"===typeof globalThis)return globalThis;try{return this||new Function("return this")()}catch(n){if("object"===typeof window)return window}}()}(),function(){t.o=function(n,e){return Object.prototype.hasOwnProperty.call(n,e)}}(),function(){t.nmd=function(n){return n.paths=[],n.children||(n.children=[]),n}}(),function(){var n={143:0};t.O.j=function(e){return 0===n[e]};var e=function(e,o){var r,i,a=o[0],l=o[1],s=o[2],u=0;if(a.some((function(e){return 0!==n[e]}))){for(r in l)t.o(l,r)&&(t.m[r]=l[r]);if(s)var c=s(t)}for(e&&e(o);u<a.length;u++)i=a[u],t.o(n,i)&&n[i]&&n[i][0](),n[i]=0;return t.O(c)},o=self["webpackChunkai"]=self["webpackChunkai"]||[];o.forEach(e.bind(null,0)),o.push=e.bind(null,o.push.bind(o))}();var o=t.O(void 0,[998],(function(){return t(4660)}));o=t.O(o)})();
//# sourceMappingURL=app.80d2b9f3.js.map