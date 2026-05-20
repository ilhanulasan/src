import { Injectable } from '@angular/core';

const BASE = '/assets/odontograph/js';
const SCRIPT_ORDER = [
  'constants.js',
  'settings.js',
  'rect.js',
  'damage.js',
  'textBox.js',
  'tooth.js',
  'menuItem.js',
  'renderer.js',
  'odontogramaGenerator.js',
  'collisionHandler.js',
  'engine.js',
] as const;

@Injectable({ providedIn: 'root' })
export class OdontographScriptLoader {
  private loadPromise: Promise<void> | null = null;

  ensureLoaded(): Promise<void> {
    if (typeof window.Engine === 'function') {
      return Promise.resolve();
    }

    this.loadPromise ??= this.loadScripts();
    return this.loadPromise;
  }

  private loadScripts(): Promise<void> {
    return SCRIPT_ORDER.reduce(
      (chain, file) => chain.then(() => this.injectScript(`${BASE}/${file}`)),
      Promise.resolve(),
    );
  }

  private injectScript(src: string): Promise<void> {
    return new Promise((resolve, reject) => {
      if (document.querySelector(`script[data-odontograph="${src}"]`)) {
        resolve();
        return;
      }

      const el = document.createElement('script');
      el.src = src;
      el.async = false;
      el.dataset['odontograph'] = src;
      el.onload = () => resolve();
      el.onerror = () => reject(new Error(`Failed to load ${src}`));
      document.body.appendChild(el);
    });
  }
}
