window.todoAudio = (function () {
  let ctx = null;

  function getContext() {
    if (!ctx) {
      ctx = new (window.AudioContext || window.webkitAudioContext)();
    }
    if (ctx.state === 'suspended') {
      ctx.resume();
    }
    return ctx;
  }

  function beep({ freq = 440, duration = 0.12, type = 'square', startFreq, endFreq, volume = 0.15, delay = 0 }) {
    const audioCtx = getContext();
    const osc = audioCtx.createOscillator();
    const gain = audioCtx.createGain();
    osc.type = type;
    const now = audioCtx.currentTime + delay;

    if (startFreq && endFreq) {
      osc.frequency.setValueAtTime(startFreq, now);
      osc.frequency.exponentialRampToValueAtTime(endFreq, now + duration);
    } else {
      osc.frequency.setValueAtTime(freq, now);
    }

    gain.gain.setValueAtTime(volume, now);
    gain.gain.exponentialRampToValueAtTime(0.001, now + duration);

    osc.connect(gain);
    gain.connect(audioCtx.destination);
    osc.start(now);
    osc.stop(now + duration);
  }

  const presets = {
    add: () => beep({ startFreq: 500, endFreq: 1000, duration: 0.15, type: 'square' }),
    complete: () => beep({ startFreq: 800, endFreq: 1600, duration: 0.18, type: 'triangle' }),
    uncomplete: () => beep({ startFreq: 600, endFreq: 300, duration: 0.15, type: 'triangle' }),
    edit: () => beep({ freq: 1200, duration: 0.08, type: 'sawtooth', volume: 0.1 }),
    delete: () => beep({ startFreq: 400, endFreq: 100, duration: 0.25, type: 'sawtooth', volume: 0.18 }),
    theme: () => beep({ startFreq: 700, endFreq: 1100, duration: 0.1, type: 'square', volume: 0.12 }),
    toggle: () => beep({ startFreq: 900, endFreq: 500, duration: 0.1, type: 'square', volume: 0.1 }),
    combo: () => {
      beep({ startFreq: 900, endFreq: 1400, duration: 0.08, type: 'square', volume: 0.14 });
      beep({ startFreq: 1100, endFreq: 1700, duration: 0.08, type: 'square', volume: 0.14, delay: 0.07 });
    },
    levelup: () => {
      beep({ startFreq: 600, endFreq: 1200, duration: 0.12, type: 'triangle', volume: 0.18 });
      beep({ startFreq: 900, endFreq: 1800, duration: 0.12, type: 'triangle', volume: 0.18, delay: 0.1 });
      beep({ startFreq: 1200, endFreq: 2400, duration: 0.2, type: 'triangle', volume: 0.18, delay: 0.2 });
    },
  };

  return {
    play(name) {
      const preset = presets[name];
      if (preset) preset();
    },
  };
})();
