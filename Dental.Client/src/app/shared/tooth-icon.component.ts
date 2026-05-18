import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-tooth-icon',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <img
      class="tooth-icon"
      src="Tooth.png"
      alt=""
      decoding="async"
      loading="lazy"
      aria-hidden="true" />
  `,
  styles: `
    :host {
      display: inline-flex;
      line-height: 0;
    }

    .tooth-icon {
      height: 1.1em;
      width: auto;
      max-height: 1.25em;
      object-fit: contain;
      vertical-align: -0.12em;
    }
  `,
})
export class ToothIconComponent {}
