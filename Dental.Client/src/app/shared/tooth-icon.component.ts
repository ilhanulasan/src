import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-tooth-icon',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <svg
      class="tooth-icon"
      viewBox="0 0 24 24"
      aria-hidden="true"
      focusable="false"
      xmlns="http://www.w3.org/2000/svg">
      <path
        fill="currentColor"
        d="M12 2C9.5 2 8 3.2 7 4.5 6.5 4 6 3.8 5.3 4 4.5 4.6 4 5.5 5 6.3 5.5 7 6 8l-.2 1.2C4.8 10 3 12.5 3 15.5 3 18 4 21 6 21c1.2 0 1.8-.8 2.3-2 .4-1 1-2 2.7-2s2.3 1 2.7 2c.5 1.2 1.1 2 2.3 2 2 0 3-3 3-5.5 0-3-1.8-5.5-4.8-6.3L19 8c.5-.7 1-1.7 1.5-2.4.5-.9 0-2-1.2-2-.7 0-1.3.3-1.7.8C16.6 3.2 15.1 2 12 2z" />
    </svg>
  `,
  styles: `
    :host {
      display: inline-flex;
      color: var(--tooth-icon-color, var(--color-mauve, #a188a6));
    }

    .tooth-icon {
      width: 1.1em;
      height: 1.1em;
      vertical-align: -0.15em;
    }
  `,
})
export class ToothIconComponent {}
