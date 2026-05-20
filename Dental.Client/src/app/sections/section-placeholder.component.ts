import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';

import { AuthService } from '../core/auth.service';
import { ToothIconComponent } from '../shared/tooth-icon.component';

@Component({
  selector: 'app-section-placeholder',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, TranslatePipe, ToothIconComponent],
  templateUrl: './section-placeholder.component.html',
  styleUrl: './section-placeholder.component.scss',
})
export class SectionPlaceholderComponent {
  private readonly route = inject(ActivatedRoute);
  readonly auth = inject(AuthService);

  readonly titleKey = (this.route.snapshot.data['titleKey'] as string | undefined) ?? 'sections.titleFallback';
}
