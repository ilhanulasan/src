import { afterNextRender, Component, inject } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, TranslatePipe],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  readonly translate = inject(TranslateService);

  constructor() {
    afterNextRender(() => {
      if (typeof localStorage === 'undefined') {
        return;
      }

      const saved = localStorage.getItem('dental-lang');
      if (saved === 'en' || saved === 'tr') {
        void this.translate.use(saved);
      }
    });
  }

  onLangChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    if (value !== 'en' && value !== 'tr') {
      return;
    }

    void this.translate.use(value);
    if (typeof localStorage !== 'undefined') {
      localStorage.setItem('dental-lang', value);
    }
  }
}
