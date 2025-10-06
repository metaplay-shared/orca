function t(e,r=3){return e>=1e9?(e/1e9).toPrecision(r)+"B":e>=1e6?(e/1e6).toPrecision(r)+"M":e>=1e3?(e/1e3).toPrecision(r)+"k":e||e===0?e.toString():void 0}export{t as a};
