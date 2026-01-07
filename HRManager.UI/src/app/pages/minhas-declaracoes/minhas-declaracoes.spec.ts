import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MinhasDeclaracoes } from './minhas-declaracoes';

describe('MinhasDeclaracoes', () => {
  let component: MinhasDeclaracoes;
  let fixture: ComponentFixture<MinhasDeclaracoes>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MinhasDeclaracoes]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MinhasDeclaracoes);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
