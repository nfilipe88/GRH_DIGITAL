import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GestaoPermissoes } from './gestao-permissoes';

describe('GestaoPermissoes', () => {
  let component: GestaoPermissoes;
  let fixture: ComponentFixture<GestaoPermissoes>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GestaoPermissoes]
    })
    .compileComponents();

    fixture = TestBed.createComponent(GestaoPermissoes);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
